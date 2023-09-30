using Stride.Core.Mathematics;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using Stride.Rendering;

public static class Utilities
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
}
