using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision;

internal struct ContactManifold {

    public const byte MaxContactPoints = 4;

    public ushort FrameId;
    public byte Count;
    public byte Position;
    public ContactPoint PointA;
    public ContactPoint PointB;
    public ContactPoint PointC;
    public ContactPoint PointD;

    public ContactPoint this[int i] {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        readonly get {
            return i switch {
                0 => PointA,
                1 => PointB,
                2 => PointC,
                3 => PointD,
                _ => throw new IndexOutOfRangeException()
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        set {
            _ = i switch {
                0 => PointA = value,
                1 => PointB = value,
                2 => PointC = value,
                3 => PointD = value,
                _ => throw new IndexOutOfRangeException()
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void AddContactPoint(ContactPoint point) {
        /*for (int i = 0; i < Count; i++) {
            ContactPoint current = this[i];
            if (current.Position.DistanceSquared(point.Position) < 0.1f) {
                this[i] = point;
                return;
            }
        }*/

        Count = (byte)Math.Min(Count + 1, MaxContactPoints);
        this[Position++ % MaxContactPoints] = point;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        Count = 0;
        Position = 0;
    }

    public void ComputeResolveImpulse(in ColliderTransform current, Quaternion<float> a, pos3 b) {
        Debug.Assert(Count > 0);
        PointA.ComputeResolveImpulse(current, a, b);

        if (Count > 3) {
            PointB.ComputeResolveImpulse(current, a, b);
            PointC.ComputeResolveImpulse(current, a, b);
            PointD.ComputeResolveImpulse(current, a, b);
        } else if (Count > 2) {
            PointB.ComputeResolveImpulse(current, a, b);
            PointC.ComputeResolveImpulse(current, a, b);
        } else if (Count > 1) {
            PointB.ComputeResolveImpulse(current, a, b);
        }
    }

}
