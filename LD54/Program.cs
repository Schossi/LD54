using Stride.Engine;
using Stride.Particles.Rendering;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using Stride.Rendering.UI;

static class Program
{
    public static Game Game;

    private static void Main(string[] _)
    {
        Game = new Game();
        Game.Script.Scheduler.Add(setup);
        Game.Run();
    }

    private static async Task setup()
    {
        var scene = Game.SceneSystem.SceneInstance.RootScene;

        setupCompositor();

        scene.Entities.Add(Environment.Create());
        scene.Entities.Add(SessionManager.Create());
        scene.Entities.Add(UI.Create());

        while (true)
        {
            await Game.Script.NextFrame();
        }
    }

    private static void setupCompositor()
    {
        var compositor = GraphicsCompositorHelper.CreateDefault(false);
        var forwardRenderer = (ForwardRenderer)compositor.SingleView;

        var cameraSlot = compositor.Cameras[0];
        var uiStage = new RenderStage("UIStage", "Main");

        compositor.RenderStages.Add(uiStage);
        compositor.RenderFeatures.Add(new UIRenderFeature
        {
            RenderStageSelectors =
            {
                new SimpleGroupToRenderStageSelector {
                    RenderStage = forwardRenderer.TransparentRenderStage,
                    EffectName = "Test",
                    RenderGroup = Enum.GetValues(typeof(RenderGroupMask)).Cast<RenderGroupMask>().Aggregate((mask, next) => mask | next) & ~RenderGroupMask.Group31
                },
                new SimpleGroupToRenderStageSelector {
                    RenderStage = uiStage,
                    EffectName = "UIStage",
                    RenderGroup = RenderGroupMask.Group31
                }
            }
        });

        var opaqueStage = compositor.RenderStages.First(x => x.Name.Equals("Opaque"));
        var transparentStage = compositor.RenderStages.First(x => x.Name.Equals("Transparent"));
        compositor.RenderFeatures.Add(new ParticleEmitterRenderFeature()
        {
            RenderStageSelectors =
            {
                new ParticleEmitterTransparentRenderStageSelector
                {
                    EffectName = "Particles",
                    OpaqueRenderStage = opaqueStage,
                    TransparentRenderStage = transparentStage,
                }
            }
        });

        compositor.Game = new SceneRendererCollection {
                new SceneCameraRenderer
                {
                    Child = forwardRenderer,
                    Camera = cameraSlot,
                    RenderMask = Enum.GetValues(typeof(RenderGroupMask)).Cast < RenderGroupMask >().Aggregate((mask, next) => mask | next) & ~ RenderGroupMask.Group31
                },
                new SceneCameraRenderer
                {
                    Camera = cameraSlot,
                    Child = new SingleStageRenderer { RenderStage = uiStage },
                    RenderMask = RenderGroupMask.Group31
                }
            };
        Game.SceneSystem.GraphicsCompositor = compositor;
    }
}