using Stride.Core.Mathematics;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using Stride.Rendering;
using Stride.Engine;
using Stride.Physics;
using Stride.Rendering.ProceduralModels;
using Stride.Games;
using Stride.UI.Panels;
using Stride.UI;

static class Utilities
{
    public static Material GetDiffuseMaterial(Color color)
    {
        var materialDescription = new MaterialDescriptor
        {
            Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
        };

        return Material.New(Program.Game.GraphicsDevice, materialDescription);
    }

    #region Stride Community Toolkit
    ///see https://github.com/VaclavElias/stride-community-toolkit

    /// <summary>
    /// Gets the time elapsed since the last game update in seconds as a single-precision floating-point number.
    /// </summary>
    /// <param name="gameTime">The IGame interface providing access to game timing information.</param>
    /// <returns>The time elapsed since the last game update in seconds.</returns>
    public static float DeltaTime(this IGame gameTime)
    {
        return (float)gameTime.UpdateTime.Elapsed.TotalSeconds;
    }

    public static Grid CreateGrid(params UIElement[] elements)
    {
        var grid = new Grid();

        grid.Children.AddRange(elements);

        return grid;
    }

    // Duplicated from Stride.Assets.Presentation.Preview
    public enum PrimitiveModelType
    {
        Sphere,
        Cube,
        Cylinder,
        Torus,
        Plane,
        Teapot,
        Cone,
        Capsule
    }

    /// <summary>
    /// Creates an entity with a primitive procedural model with a primitive mesh renderer and adds appropriate collider except for Torus, Teapot and Plane.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="type"></param>
    /// <param name="entityName"></param>
    /// <param name="material"></param>
    /// <param name="includeCollider">Adds a default collider except for Torus, Teapot and Plane. Default true.</param>
    /// <returns></returns>
    public static Entity CreatePrimitive(this Game game, PrimitiveModelType type, string entityName = null, Material material = null, bool includeCollider = true)
    {
        var proceduralModel = getProceduralModel(type);

        var model = proceduralModel.Generate(game.Services);

        model.Materials.Add(material);

        var entity = new Entity(entityName) { new ModelComponent(model) };

        if (!includeCollider) return entity;

        var colliderShape = getColliderShape(type);

        if (colliderShape is null) return entity;

        var collider = new RigidbodyComponent();

        collider.ColliderShapes.Add(colliderShape);

        entity.Add(collider);

        return entity;
    }

    private static PrimitiveProceduralModelBase getProceduralModel(PrimitiveModelType type)
        => type switch
        {
            PrimitiveModelType.Plane => new PlaneProceduralModel(),
            PrimitiveModelType.Sphere => new SphereProceduralModel(),
            PrimitiveModelType.Cube => new CubeProceduralModel(),
            PrimitiveModelType.Cylinder => new CylinderProceduralModel(),
            PrimitiveModelType.Torus => new TorusProceduralModel(),
            PrimitiveModelType.Teapot => new TeapotProceduralModel(),
            PrimitiveModelType.Cone => new ConeProceduralModel(),
            PrimitiveModelType.Capsule => new CapsuleProceduralModel(),
            _ => throw new InvalidOperationException(),
        };

    private static IInlineColliderShapeDesc getColliderShape(PrimitiveModelType type)
        => type switch
        {
            PrimitiveModelType.Plane => null,
            PrimitiveModelType.Sphere => new SphereColliderShapeDesc(),
            PrimitiveModelType.Cube => new BoxColliderShapeDesc(),
            PrimitiveModelType.Cylinder => new CylinderColliderShapeDesc(),
            PrimitiveModelType.Torus => null,
            PrimitiveModelType.Teapot => null,
            PrimitiveModelType.Cone => new ConeColliderShapeDesc(),
            PrimitiveModelType.Capsule => new CapsuleColliderShapeDesc(),
            _ => throw new InvalidOperationException(),
        };
    #endregion
}
