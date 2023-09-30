using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using Stride.Rendering.ProceduralModels;

class Dozer : SyncScript
{
    const float AREA = 4f;

    const float SPEED = 5;
    const float SPEED_ROTATE = 3;
    const float SPEED_SP = 8;
    const float SPEED_ROTATE_SP = 5;

    const float SPECIAL_DURATION = 5f;
    const float SPECIAL_COOLDOWN = 20f;

    public static Dozer Instance;

    public bool IsSpecialActive => _specialTime > 0f;
    public bool IsSpecialReady => _specialTime <= -SPECIAL_COOLDOWN;
    public float SpecialRatio
    {
        get
        {
            if (IsSpecialActive || IsSpecialReady)
                return 1;
            return -_specialTime / SPECIAL_COOLDOWN;
        }
    }

    public float Speed => IsSpecialActive ? SPEED_SP : SPEED;
    public float SpeedRotate => IsSpecialActive ? SPEED_ROTATE_SP : SPEED_ROTATE;

    private float _rotation;

    private float _specialTime;

    public override void Update()
    {
        if (!SessionManager.Instance.IsPlaying)
            return;

        updateMovement();
        updateSpecial();
    }

    private void updateMovement()
    {
        var movement = new Vector3();
        if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W))
        {
            movement = Speed * Entity.Transform.LocalMatrix.Forward * Game.DeltaTime();
        }
        else if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
        {
            movement = -Speed * Entity.Transform.LocalMatrix.Forward * Game.DeltaTime();
        }
        Entity.Transform.Position = new Vector3(MathUtil.Clamp(Entity.Transform.Position.X + movement.X, -AREA, AREA), Entity.Transform.Position.Y, MathUtil.Clamp(Entity.Transform.Position.Z + movement.Z, -AREA, AREA));

        if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
        {
            _rotation += SpeedRotate * Game.DeltaTime();
        }
        else if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
        {
            _rotation -= SpeedRotate * Game.DeltaTime();
        }
        Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(_rotation, 0, 0);
    }

    private void updateSpecial()
    {
        if (_specialTime > 0)
        {
            _specialTime -= Game.DeltaTime();
            if (_specialTime <= 0)
            {
                endSpecial();
            }
        }
        else
        {
            if (IsSpecialReady)
            {
                if (Input.IsKeyDown(Keys.Space))
                {
                    _specialTime = SPECIAL_DURATION;
                    startSpecial();
                }
            }
            else
            {
                _specialTime -= Game.DeltaTime();
            }
        }
    }

    private void startSpecial()
    {
    }
    private void endSpecial()
    {
    }

    public static Entity Create()
    {
        var game = Program.Game;

        Instance = new();
        var dozerEntity = new Entity("Dozer") { Instance };

        var cubeModel = new CubeProceduralModel
        {
            Size = new Vector3(1, 1, 1),
            MaterialInstance = { Material = Utilities.GetDiffuseMaterial(Color.Yellow) }
        };
        var cubeCollider = new RigidbodyComponent
        {
            IsKinematic = true,
            CollisionGroup = CollisionFilterGroups.CharacterFilter
        };
        cubeCollider.ColliderShapes.Add(new BoxColliderShapeDesc() { Size = new Vector3(1, 1, 1) });
        var cubeEntity = new Entity("Cube") { new ModelComponent(cubeModel.Generate(game.Services)), cubeCollider };
        cubeEntity.Transform.Scale = new(2, 2, 2);
        cubeEntity.Transform.Position = new(0, 1f, 0);

        dozerEntity.AddChild(cubeEntity);

        return dozerEntity;
    }
}