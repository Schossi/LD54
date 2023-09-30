﻿using Stride.Engine;

class SessionManager : SyncScript
{
    public enum ManagerState { Start, Playing, GameOver }

    public static SessionManager Instance { get; private set; }

    public ManagerState State { get; private set; }
    public float SessionTime { get; private set; }
    public bool IsPlaying => State == ManagerState.Playing;

    public event Action<ManagerState> StateChanged;

    public override void Update()
    {
        if (State == ManagerState.Playing)
        {
            SessionTime += Game.DeltaTime();
        }
    }

    public void StartSession()
    {
        if (Dozer.Instance != null)
            Entity.RemoveChild(Dozer.Instance.Entity);
        if (GarbageFactory.Instance != null)
            Entity.RemoveChild(GarbageFactory.Instance.Entity);

        changeState(ManagerState.Playing);

        Entity.AddChild(Dozer.Create());
        Entity.AddChild(GarbageFactory.Create());
    }
    public void EndSession()
    {
        changeState(ManagerState.GameOver);
    }

    private void changeState(ManagerState state)
    {
        State = state;
        StateChanged?.Invoke(state);
    }

    public static Entity Create()
    {
        Instance = new();

        return new Entity("Manager") { Instance };
    }
}
