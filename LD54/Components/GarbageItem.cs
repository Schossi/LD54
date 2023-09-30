using Stride.Engine;

class GarbageItem : SyncScript
{
    public override void Update()
    {
        if (Entity.Transform.Position.Y < -1)
        {
            GarbageFactory.Instance.RemoveItem(this);
        }
    }

    public static Entity Create()
    {
        var game = Program.Game;

        var debugCube = game.CreatePrimitive(Utilities.PrimitiveModelType.Cube);
        debugCube.Add(new GarbageItem());

        return debugCube;
    }
}
