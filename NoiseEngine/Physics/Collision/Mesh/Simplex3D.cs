using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal struct Simplex3D {

    public int Dimensions;
    public SupportPoint A;
    public SupportPoint B;
    public SupportPoint C;
    public SupportPoint D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Span<SupportPoint> span) {
        span[0] = A;
        span[1] = B;
        span[2] = C;
        span[3] = D;
    }

}
