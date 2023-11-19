using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Physics.Collision;

[StructLayout(LayoutKind.Sequential)]
internal struct ContactManifold {

    public const byte MaxContactPoints = 4;

    // NOTE: Points must be in order, because of unsafe behavior in this struct.
    public ContactPoint PointA;
    public ContactPoint PointB;
    public ContactPoint PointC;
    public ContactPoint PointD;

    public ushort FrameId;
    public byte Count;
    public byte Position;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static ref ContactPoint GetPointRef(ref ContactManifold manifold, int i) {
        Debug.Assert(i >= 0 && i < MaxContactPoints);
        return ref Unsafe.Add(ref manifold.PointA, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void AddContactPoint(ContactPoint point) {
        /*for (int i = 0; i < Count; i++) {
            ref ContactPoint current = ref GetPointRef(ref this, i);
            if (current.Position.DistanceSquared(point.Position) < 0.1f) {
                current = point with { PreviousNormalImpulse = current.PreviousNormalImpulse, Bias = current.Bias };
                return;
            }
        }*/

        //Count = (byte)Math.Min(Count + 1, MaxContactPoints);
        /*if (Count == MaxContactPoints) {
            for (int i = 0; i < Count; i++) {
                ref ContactPoint current = ref GetPointRef(ref this, i);
                if (current.Depth == 0) {
                    current = point;
                    return;
                }
            }
        }*/

        //GetPointRef(ref this, Position++ % MaxContactPoints) = point;

        int index = Count;
        if (index == MaxContactPoints)
            index = SortPoints(point);
        else
            Count++;

        GetPointRef(ref this, index) = point;
    }

    public void RemoveContactPoint(int index) {
        int lastUsedIndex = --Count;
        if (index != lastUsedIndex)
            GetPointRef(ref this, index) = GetPointRef(ref this, lastUsedIndex);
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

    private int SortPoints(in ContactPoint newPoint) {
        int maxDepthIndex = -1;
        float maxDepth = newPoint.Depth;

        for (byte i = 0; i < Count; i++) {
            ref ContactPoint current = ref GetPointRef(ref this, i);
            if (current.Depth < maxDepth) {
                maxDepthIndex = i;
                maxDepth = current.Depth;
            }
        }

        return new float4(
            FindRes(newPoint, maxDepthIndex, 0, 1, 3, 2),
            FindRes(newPoint, maxDepthIndex, 2, 0, 3, 2),
            FindRes(newPoint, maxDepthIndex, 2, 0, 3, 1),
            FindRes(newPoint, maxDepthIndex, 3, 0, 2, 1)
        ).MaxComponentIndex();
    }

    private float FindRes(
        in ContactPoint newPoint, int maxDepthIndex, int index, int pointIndexA, int pointIndexB, int pointIndexC
    ) {
        if (maxDepthIndex == index)
            return 0;

        float3 a = (newPoint.Position - GetPointRef(ref this, pointIndexA).Position).ToFloat();
        float3 b = (
            GetPointRef(ref this, pointIndexB).Position - GetPointRef(ref this, pointIndexC).Position
        ).ToFloat();
        return a.Cross(b).MagnitudeSquared();
    }

}
