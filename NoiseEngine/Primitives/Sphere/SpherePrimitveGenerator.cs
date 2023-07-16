using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Primitives.Sphere;

internal static class SpherePrimitveGenerator {

    public static Mesh GenerateMesh(PrimitiveCreatorShared shared, uint resolution, SphereType type) {
        return type switch {
            SphereType.Icosphere => new IcosphereMesh().GenerateMesh(shared, resolution),
            _ => throw new NotImplementedException(),
        };
    }

}
