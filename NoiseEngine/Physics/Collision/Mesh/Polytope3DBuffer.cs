using NoiseEngine.Collections;

namespace NoiseEngine.Physics.Collision.Mesh;

internal readonly record struct Polytope3DBuffer(FastList<PolytopeFace> Faces, FastList<(int, int)> Edges);
