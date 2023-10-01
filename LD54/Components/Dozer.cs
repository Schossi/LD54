using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using Stride.Rendering.ProceduralModels;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Particles.Components;
using Stride.Particles.Initializers;
using Stride.Particles.Spawners;
using Stride.Particles;
using System.Media;
using FreeImageAPI.Metadata;
using Stride.Audio;

class Dozer : SyncScript
{
    const float AREA = 4f;

    const float SPEED = 5;
    const float SPEED_ROTATE = 3;
    const float SPEED_SP = 15;
    const float SPEED_ROTATE_SP = 1;

    const float SPECIAL_DURATION = 0.75f;
    const float SPECIAL_COOLDOWN = 15f;

    public static Dozer Instance;

    public ParticleSystemComponent SpecialParticles { get; private set; }

    public bool IsSpecialActive => _specialTime > 0f;
    public bool IsSpecialReady => _specialTime <= -SPECIAL_COOLDOWN;
    public float SpecialRatio
    {
        get
        {
            if (IsSpecialReady)
                return 1;
            else if (IsSpecialActive)
                return _specialTime / SPECIAL_DURATION;
            else
                return -_specialTime / SPECIAL_COOLDOWN;
        }
    }

    public float Speed => IsSpecialActive ? SPEED_SP : SPEED;
    public float SpeedRotate => IsSpecialActive ? SPEED_ROTATE_SP : SPEED_ROTATE;

    private float _rotation = MathF.PI;
    private float _specialTime;
    private RigidbodyComponent _body;
    private Utilities.CachedSound _moveSound, _moveStepSound, _moveSpecialSound;
    private float _moveTime;

    public override void Start()
    {
        base.Start();

        _moveSound = new Utilities.CachedSound($"{AppContext.BaseDirectory}Resources\\move.wav");
        _moveStepSound = new Utilities.CachedSound($"{AppContext.BaseDirectory}Resources\\moveStep.wav");
        _moveSpecialSound = new Utilities.CachedSound($"{AppContext.BaseDirectory}Resources\\moveSpecial.wav");

        _body = Entity.FindChild("Cube").Get<RigidbodyComponent>();
        _body.Collisions.CollectionChanged += collisions_CollectionChanged;
    }

    private void collisions_CollectionChanged(object sender, Stride.Core.Collections.TrackingCollectionChangedEventArgs e)
    {
        if (!IsSpecialActive)
            return;

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            var collision = (Collision)e.Item;

            Entity other;
            if (collision.ColliderA == _body)
                other = collision.ColliderB.Entity;
            else
                other = collision.ColliderA.Entity;

            other.Get<GarbageItem>()?.SpecialRemove();
        }
    }

    public override void Update()
    {
        if (!SessionManager.Instance.IsPlaying)
            return;

        updateMovement();
        updateSpecial();
    }

    private void updateMovement()
    {
        if (!IsSpecialActive)
        {
            if (Input.IsKeyPressed(Keys.Up) || Input.IsKeyPressed(Keys.W) ||
                Input.IsKeyPressed(Keys.Down) || Input.IsKeyPressed(Keys.S))
            {
                _moveSound.Play();
                _moveTime = 0f;
            }
            else if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W) ||
                Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
            {
                _moveTime += Game.DeltaTime();
                if (_moveTime > 0.15f)
                {
                    _moveStepSound.Play();
                    _moveTime = 0f;
                }
            }
        }

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
                if (Input.IsKeyDown(Keys.Space) || Input.IsKeyDown(Keys.RightCtrl))
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
        SpecialParticles.ParticleSystem.Play();

        foreach (var collision in _body.Collisions)
        {
            Entity other;
            if (collision.ColliderA == _body)
                other = collision.ColliderB.Entity;
            else
                other = collision.ColliderA.Entity;

            other.Get<GarbageItem>()?.SpecialRemove();
        }

        _moveSpecialSound.Play();
        _moveTime = 0f;
    }
    private void endSpecial()
    {
        SpecialParticles.ParticleSystem.Stop();
    }

    private static Model _model;
    public static Entity Create()
    {
        var game = Program.Game;

        var particleComponent = new ParticleSystemComponent() { Color = new Color4(1.0f, 0.0f, 0.0f, 0.2f) };
        var emitter = new ParticleEmitter() { ParticleLifetime = new(0.3f, 2f) };
        emitter.Spawners.Add(new SpawnerPerSecond() { SpawnCount = 100 });
        emitter.Initializers.Add(new InitialSizeSeed() { RandomSize = new(0.2f, 1.5f) });
        emitter.Initializers.Add(new InitialVelocitySeed() { VelocityMin = new(-3, -3, -3), VelocityMax = new(3, 3, 3) });
        particleComponent.ParticleSystem.Emitters.Add(emitter);
        particleComponent.ParticleSystem.Stop();

        var particleEntity = new Entity("Particles") { particleComponent };
        particleEntity.Transform.Position = new(0, 1, 0);

        Instance = new() { SpecialParticles = particleComponent };

        var dozerEntity = new Entity("Dozer") { Instance };
        dozerEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathF.PI, 0, 0);
        dozerEntity.AddChild(particleEntity);

        //PHYSICS
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

        var cubeEntity = new Entity("Cube") { cubeCollider };// new ModelComponent(cubeModel.Generate(game.Services)), cubeCollider };
        cubeEntity.Transform.Scale = new(2, 2, 2);
        cubeEntity.Transform.Position = new(0, 1f, 0);
        dozerEntity.AddChild(cubeEntity);

        //VISUALS
        if (_model == null)
        {
            using var stream = new FileStream($"{AppContext.BaseDirectory}Resources\\Palette.png", FileMode.Open, FileAccess.Read);
            var texture = Texture.Load(game.GraphicsDevice, stream);
            var materialDescriptor = new MaterialDescriptor()
            {
                Attributes =
            {
                Diffuse=new MaterialDiffuseMapFeature(new ComputeTextureColor(texture)),
                DiffuseModel=new MaterialDiffuseLambertModelFeature()
            }
            };

            var meshConverter = new Stride.Importer.FBX.MeshConverter(Utilities.DebugLogger.Instance) { };
            _model = meshConverter.Convert($"{AppContext.BaseDirectory}Resources\\Dozer.fbx", string.Empty, new Dictionary<string, int>());

            //save and load to properly initialize
            game.Content.Save("D", _model);
            game.Content.Load<Model>("D");

            _model.Materials.Add(new MaterialInstance(Material.New(game.GraphicsDevice, materialDescriptor)));
            _model.Meshes.ForEach(m => m.NodeIndex = 0);//node index is at 1 for some reason, which does not exist(no skeleton?)
        }

        var entity = new Entity("Visual") { new ModelComponent(_model) };
        entity.Transform.Scale = new(1f, 1f, 1f);
        entity.Transform.Position = new(0f, 0f, 0f);
        entity.Transform.Rotation = Quaternion.RotationY(MathF.PI);
        dozerEntity.AddChild(entity);

        return dozerEntity;
    }
}