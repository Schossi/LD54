using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Processors;
using Stride.Physics;
using Stride.Rendering.Colors;
using Stride.Rendering.Lights;
using Stride.Rendering.ProceduralModels;

class Environment : SyncScript
{
    public static Environment Instance;

    public CameraComponent Camera { get; set; }

    public LightComponent DirectionalLight { get; set; }
    public LightComponent AmbientLight { get; set; }

    public override void Update()
    {
        Camera.Entity.Transform.Position = new(0, 10, 11);
        Camera.Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(
            MathUtil.DegreesToRadians(0),
            MathUtil.DegreesToRadians(-45),
            MathUtil.DegreesToRadians(0)
        );

        DirectionalLight.Entity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-30.0f)) * Quaternion.RotationY(MathUtil.DegreesToRadians(-45f));
    }

    public static Entity Create()
    {
        var game = Program.Game;

        Instance = new()
        {
            Camera = new()
            {
                Projection = CameraProjectionMode.Perspective,
                Slot = game.SceneSystem.GraphicsCompositor.Cameras[0].ToSlotId()
            },

            DirectionalLight = new()
            {
                Intensity = 1.0f,
                Type = new LightDirectional
                {
                    Color = new ColorRgbProvider(Color.WhiteSmoke),
                    Shadow =
                {
                    Enabled = true,
                    Size = LightShadowMapSize.Large,
                    Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter5x5 },
                }
                }
            },
            AmbientLight = new()
            {
                Intensity = 0.5f,
                Type = new LightAmbient() { Color = new ColorRgbProvider(Color.White) }
            }
        };

        var environmentEntity = new Entity("Environment") { Instance };
        environmentEntity.AddChild(new Entity("Camera") { Instance.Camera });
        environmentEntity.AddChild(new Entity("DirectionalLight") { Instance.DirectionalLight });
        environmentEntity.AddChild(new Entity("AmbientLight") { Instance.AmbientLight });

        var groundModel = new PlaneProceduralModel
        {
            Size = new Vector2(10.0f, 10.0f),
            MaterialInstance = { Material = Utilities.GetDiffuseMaterial(Color.SandyBrown) }
        };

        var ground = new StaticColliderComponent() { Friction = 2f };
        ground.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new Vector3(10, 1, 10),
            LocalOffset = new Vector3(0, -0.5f, 0)
        });

        var walls = new StaticColliderComponent() { Friction = 0f, CanCollideWith = CollisionFilterGroupFlags.DefaultFilter };
        walls.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new(10, 10, 1),
            LocalOffset = new(0, 0, -5.5f),
        });
        walls.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new(1, 10, 10),
            LocalOffset = new(-5.5f, 0, 0),
        });
        walls.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new(1, 10, 10),
            LocalOffset = new(5.5f, 0, 0),
        });

        walls.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new(2, 10, 2),
            LocalOffset = new(5, 0, -5),
            LocalRotation = Quaternion.RotationY(45)
        });
        walls.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new(2, 10, 2),
            LocalOffset = new(-5, 0, -5),
            LocalRotation = Quaternion.RotationY(45)
        });

        var groundEntity = new Entity("Ground") { new ModelComponent(groundModel.Generate(game.Services)), ground, walls };

        environmentEntity.AddChild(groundEntity);

        var borderMaterial = Utilities.GetDiffuseMaterial(Color.DarkGray);
        var backBorder = game.CreatePrimitive(Utilities.PrimitiveModelType.Cube, material: borderMaterial, includeCollider: false);
        backBorder.Transform.Scale = new(12, 2, 1);
        backBorder.Transform.Position = new(0, 0, -6);
        var leftBorder = game.CreatePrimitive(Utilities.PrimitiveModelType.Cube, material: borderMaterial, includeCollider: false);
        leftBorder.Transform.Scale = new(1, 2, 11);
        leftBorder.Transform.Position = new(-5.5f, 0, -0.5f);
        var rightBorder = game.CreatePrimitive(Utilities.PrimitiveModelType.Cube, material: borderMaterial, includeCollider: false);
        rightBorder.Transform.Scale = new(1, 2, 11);
        rightBorder.Transform.Position = new(5.5f, 0, -0.5f);
        var bottomBorder = game.CreatePrimitive(Utilities.PrimitiveModelType.Cube, material: borderMaterial, includeCollider: false);
        bottomBorder.Transform.Scale = new(12, 1, 1);
        bottomBorder.Transform.Position = new(0, -0.51f, 4.5f);

        environmentEntity.AddChild(backBorder);
        environmentEntity.AddChild(leftBorder);
        environmentEntity.AddChild(rightBorder);
        environmentEntity.AddChild(bottomBorder);

        return environmentEntity;
    }
}
