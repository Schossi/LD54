using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Processors;
using Stride.Physics;
using Stride.Rendering.Colors;
using Stride.Rendering.Lights;
using Stride.Rendering.ProceduralModels;

public class Environment : SyncScript
{
    public static Environment Instance;

    public CameraComponent Camera { get; set; }

    public LightComponent DirectionalLight { get; set; }
    public LightComponent AmbientLight { get; set; }

    public override void Update()
    {
        Camera.Entity.Transform.Position = new(0, 8, 10);
        Camera.Entity.Transform.Rotation = Quaternion.RotationYawPitchRoll(
            MathUtil.DegreesToRadians(0),
            MathUtil.DegreesToRadians(-40),
            MathUtil.DegreesToRadians(0)
        );

        DirectionalLight.Entity.Transform.Rotation = Quaternion.RotationX(MathUtil.DegreesToRadians(-30.0f)) * Quaternion.RotationY(MathUtil.DegreesToRadians(-180.0f));
    }

    public static void Setup(Scene scene)
    {
        var game = Program.Game;

        Instance = new();

        Instance.Camera = new()
        {
            Projection = CameraProjectionMode.Perspective,
            Slot = game.SceneSystem.GraphicsCompositor.Cameras[0].ToSlotId()
        };

        Instance.DirectionalLight = new()
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
        };
        Instance.AmbientLight = new()
        {
            Intensity = 0.5f,
            Type = new LightAmbient() { Color = new ColorRgbProvider(Color.White) }
        };

        var environmentEntity = new Entity("Environment") { Instance };
        environmentEntity.AddChild(new Entity("Camera") { Instance.Camera });
        environmentEntity.AddChild(new Entity("DirectionalLight") { Instance.DirectionalLight });
        environmentEntity.AddChild(new Entity("AmbientLight") { Instance.AmbientLight });

        var groundModel = new PlaneProceduralModel
        {
            Size = new Vector2(10.0f, 10.0f),
            MaterialInstance = { Material = Utilities.GetDiffuseMaterial(Color.Gray) }
        };

        var groundCollider = new StaticColliderComponent();

        groundCollider.ColliderShapes.Add(new BoxColliderShapeDesc()
        {
            Size = new Vector3(10, 1, 10),
            LocalOffset = new Vector3(0, -0.5f, 0)
        });

        var groundEntity = new Entity("Ground") { new ModelComponent(groundModel.Generate(game.Services)), groundCollider };

        environmentEntity.AddChild(groundEntity);

        scene.Entities.Add(environmentEntity);
    }
}
