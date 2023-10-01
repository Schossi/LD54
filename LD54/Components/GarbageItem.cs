using Stride.Engine;
using Stride.Physics;
using Stride.Rendering;

class GarbageItem : SyncScript
{
    public override void Update()
    {
        if (Entity.Transform.Position.Y < -1f || Entity.Transform.Position.Z > 7f)
        {
            GarbageFactory.Instance.RemoveItem(this);
        }
    }

    public void SpecialRemove()
    {
        GarbageFactory.Instance.RemoveItem(this);
    }

    public static Entity Create(Model model)
    {
        var collider = new RigidbodyComponent();
        collider.ColliderShapes.Add(new BoxColliderShapeDesc());

        var entity = new Entity("Item") { new ModelComponent(model), collider, new GarbageItem() };

        return entity;
    }
}
