using Stride.Engine;

class GarbageFactory : SyncScript
{
    public const int ITEMS_MAX = 40;

    public static GarbageFactory Instance;

    private float _time;

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

    public override void Update()
    {
        if (!SessionManager.Instance.IsPlaying)
            return;

        _time += Game.DeltaTime();
        if (_time > 0.5f)
        {
            _time = 0f;
            spawn();
        }
    }

    public void RemoveItem(GarbageItem item)
    {
        Entity.RemoveChild(item.Entity);

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
            var item = GarbageItem.Create();

            item.Transform.Position = new(Random.Shared.NextSingle() * 5 - 2.5f, 8, Random.Shared.NextSingle() * 5 - 2.5f);

            Entity.AddChild(item);
        }
    }

    public static Entity Create()
    {
        Instance = new();

        return new Entity("GarbageFactory") { Instance };
    }
}
