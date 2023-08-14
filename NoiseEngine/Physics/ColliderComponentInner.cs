using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Physics;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct ColliderComponentInner {

    [FieldOffset(0)]
    public readonly float SphereRadius;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ColliderComponentInner(SphereCollider sphereCollider) {
        SphereRadius = sphereCollider.Radius;
    }

}
