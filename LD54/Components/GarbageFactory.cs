using Stride.Engine;
using Stride.Rendering.Materials;
using Stride.Rendering;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Graphics;
using Stride.Particles.Components;
using Stride.Particles;
using Stride.Particles.Spawners;
using Stride.Particles.Initializers;
using Stride.Core.Mathematics;
using System.Media;

class GarbageFactory : SyncScript
{
    public const int ITEMS_MAX = 40;
    public const float SPAWN_RADIUS = 7;

    public static GarbageFactory Instance;

    public Model BoxModel { get; private set; }

    public int ActiveCount => Entity.Transform.Children.Count;
    public int DestroyedCount { get; private set; }

    public float SpaceRatio
    {
        get
        {
            if (ActiveCount > ITEMS_MAX)
                return 1;
            return (float)ActiveCount / ITEMS_MAX;
        }
    }

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Utilities.CachedSound _dropPlayer, _destroyPlayer;

    public override void Start()
    {
        base.Start();

        _dropPlayer = new Utilities.CachedSound($"{AppContext.BaseDirectory}Resources\\drop.wav");
        _destroyPlayer = new Utilities.CachedSound($"{AppContext.BaseDirectory}Resources\\destroy.wav");

        //got some strange enumeration error once, running on the scheduler might prevent that?
        Program.Game.Script.Scheduler.Add(() => spawning(_cancellationTokenSource.Token));
        //Task.Run(() => spawning(_cancellationTokenSource.Token));

        SessionManager.Instance.StateChanged += sessionStateChanged;
    }

    public override void Update()
    {
#if DEBUG
        if (Input.IsKeyDown(Stride.Input.Keys.Q))
            Program.Game.Script.Scheduler.Add(() => showParticles(new(0, 2, 0)));
#endif
    }

    private void sessionStateChanged(SessionManager.ManagerState state)
    {
        if (state != SessionManager.ManagerState.Playing)
        {
            _cancellationTokenSource.Cancel();
            SessionManager.Instance.StateChanged -= sessionStateChanged;
        }
    }

    private async Task spawning(CancellationToken token)
    {
        //DEBUG GAME OVER
        //for (int i = 0; i < 50; i++)
        //{
        //    spawn();
        //    await Task.Delay(10, token);
        //}

        //spawn a bunch at the start
        for (int i = 0; i < 25; i++)
        {
            spawn();
            await Task.Delay(100, token);
        }

        //spawn one every second
        while (SessionManager.Instance.SessionTime < 30)
        {
            spawn();
            await Task.Delay(1000, token);
        }

        //spawn 15
        for (int i = 0; i < 15; i++)
        {
            spawn();
            await Task.Delay(100, token);
        }

        //spawn one every 0.75 seconds
        while (SessionManager.Instance.SessionTime < 60)
        {
            spawn();
            await Task.Delay(750, token);
        }

        //spawn 10
        for (int i = 0; i < 10; i++)
        {
            spawn();
            await Task.Delay(100, token);
        }

        while (SessionManager.Instance.SessionTime < 90)
        {
            spawn();
            await Task.Delay(600, token);
        }

        //spawn 5
        for (int i = 0; i < 5; i++)
        {
            spawn();
            await Task.Delay(100, token);
        }

        while (SessionManager.Instance.SessionTime < 120)
        {
            spawn();
            await Task.Delay(450, token);
        }

        while (SessionManager.Instance.SessionTime < 150)
        {
            spawn();
            await Task.Delay(400, token);
        }

        while (true)
        {
            spawn();
            await Task.Delay(350, token);
        }
    }

    public void RemoveItem(GarbageItem item)
    {
        Entity.RemoveChild(item.Entity);

        Program.Game.Script.Scheduler.Add(() => showParticles(item.Entity.Transform.Position));

        _destroyPlayer.Play();

        if (!SessionManager.Instance.IsPlaying)
            return;

        DestroyedCount++;
    }

    private void spawn()
    {
        if (ActiveCount >= ITEMS_MAX)
        {
            SessionManager.Instance.EndSession();
        }
        else
        {
            var item = GarbageItem.Create(BoxModel);

            item.Transform.Position = new(Random.Shared.NextSingle() * SPAWN_RADIUS - SPAWN_RADIUS / 2f, 8, Random.Shared.NextSingle() * SPAWN_RADIUS - SPAWN_RADIUS / 2f);
            item.Transform.Scale = new(0.5f + Random.Shared.NextSingle(), 0.5f + Random.Shared.NextSingle(), 0.5f + Random.Shared.NextSingle());

            Entity.AddChild(item);

            _dropPlayer.Play();
        }
    }

    private async Task showParticles(Vector3 position)
    {
        var particleComponent = new ParticleSystemComponent() { Color = new Color4(0.8f, 0.8f, 0.8f, 0.5f) };
        var emitter = new ParticleEmitter() { ParticleLifetime = new(0.1f, 1f) };
        emitter.Spawners.Add(new SpawnerBurst() { SpawnCount = 20, LoopCondition = SpawnerLoopCondition.OneShot });
        emitter.Initializers.Add(new InitialSizeSeed() { RandomSize = new(0.1f, 0.5f) });
        emitter.Initializers.Add(new InitialVelocitySeed() { VelocityMin = new(-1, -1, -1), VelocityMax = new(1, 1, 1) });
        particleComponent.ParticleSystem.Emitters.Add(emitter);

        var entity = new Entity("Particles") { particleComponent };
        entity.Transform.Position = position;

        var scene = Program.Game.SceneSystem.SceneInstance.RootScene;

        scene.Entities.Add(entity);
        particleComponent.ParticleSystem.Play();
        await Task.Delay(1000);
        scene.Entities.Remove(entity);
    }

    private static Model _model;
    public static Entity Create()
    {
        var game = Program.Game;

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
            _model = meshConverter.Convert($"{AppContext.BaseDirectory}Resources\\Box.fbx", string.Empty, new Dictionary<string, int>());

            //save and load to properly initialize
            game.Content.Save("B", _model);
            game.Content.Load<Model>("B");

            _model.Materials.Add(new MaterialInstance(Material.New(game.GraphicsDevice, materialDescriptor)));
            _model.Meshes.ForEach(m => m.NodeIndex = 0);//node index is at 1 for some reason, which does not exist(no skeleton?)
        }

        Instance = new() { BoxModel = _model };

        return new Entity("GarbageFactory") { Instance };
    }
}
