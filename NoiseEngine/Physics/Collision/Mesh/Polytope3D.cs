using NoiseEngine.Collections;
using NoiseEngine.Collections.Span;
using NoiseEngine.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal ref struct Polytope3D {

    private SpanList<SupportPoint> vertices;
    private SpanList<int> faceIds;

    public readonly Span<SupportPoint> Vertices => vertices.Data;
    public readonly FastList<PolytopeFace> Faces { get; }
    public readonly FastList<(int, int)> Edges { get; }

    public Polytope3D(
        Span<SupportPoint> vertices, int verticesCount, Polytope3DBuffer buffer, Span<int> faceIds,
        int faceIdsCount
    ) {
        this.vertices = new SpanList<SupportPoint>(vertices, verticesCount);
        Faces = buffer.Faces;
        Edges = buffer.Edges;
        this.faceIds = new SpanList<int>(faceIds, faceIdsCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveOrAddEdge(FastList<(int, int)> edges, (int, int) edge) {
        for (int i = 0; i < edges.Count; i++) {
            (int a, int b) = edges[i];
            if (a == edge.Item2 && b == edge.Item1) {
                edges.RemoveLastWithoutClear(1);
                if (edges.Count > 0)
                    edges[i] = edges[edges.Count];
                return;
            }
        }

        edges.Add(edge);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeEdges(
        ref SpanList<(int, int)> edges, Span<PolytopeFace> faces, Span<SupportPoint> vertices,
        in SupportPoint supportPoint, int faceId, int oppVertexId
    ) {
        ref PolytopeFace face = ref faces[faceId];
        if (face.IsDeleted)
            return;

        if (face.Normal.Dot(supportPoint.Value - vertices[face.VertexId.X].Value) < 0) {
            edges.Add((faceId, oppVertexId));
            return;
        }

        face.IsDeleted = true;

        int a = (oppVertexId + 2) % 3;
        int neighborIdX = face.NeighborId[a];
        int neighborIdY = face.NeighborId[oppVertexId];

        int oppVertexIdX = faces[neighborIdX].NextCcwInner(face.VertexId[a]);
        int oppVertexIdY = faces[neighborIdY].NextCcwInner(face.VertexId[oppVertexId]);

        ComputeEdges(ref edges, faces, vertices, in supportPoint, neighborIdX, oppVertexIdX);
        ComputeEdges(ref edges, faces, vertices, in supportPoint, neighborIdY, oppVertexIdY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryIterate(out int faceId) {
        /*if (faces.Count == i) {
            faceId = default;
            return false;
        }

        faceId = i++;
        return true;*/

        if (faceIds.Count == 0) {
            faceId = default;
            return false;
        }

        faceId = faceIds.buffer[0];
        faceIds.buffer[0] = faceIds.buffer[--faceIds.Count];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly (PolytopeFace face, int faceId) ClosestFaceToOrigin() {
        FastList<PolytopeFace> faces = Faces;
        PolytopeFace face = new PolytopeFace();

        int id = 0;
        for (int i = 0; i < faces.Count; i++) {
            ref PolytopeFace f = ref faces.GetRef(i);
            if ((f.Distance < face.Distance || face.Distance == 0) && !face.IsDeleted) {
                face = f;
                id = i;
            }
        }

        return (face, id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(SupportPoint supportPoint) {
        FastList<(int, int)> edges = Edges;

        Span<SupportPoint> vertices = Vertices;
        FastList<PolytopeFace> faces = Faces;

        for (int i = 0; i < faces.Count; i++) {
            PolytopeFace f = faces[i];
            float dot = f.Normal.Dot(supportPoint.Value - vertices[f.VertexId.X].Value);
            if (dot <= 0)
                continue;

            RemoveOrAddEdge(edges, (f.VertexId.X, f.VertexId.Y));
            RemoveOrAddEdge(edges, (f.VertexId.Y, f.VertexId.Z));
            RemoveOrAddEdge(edges, (f.VertexId.Z, f.VertexId.X));

            faces.RemoveLastWithoutClear(1);
            if (faces.Count > 0)
                faces[i] = faces[faces.Count];
            i--;
        }

        // Add vertex.
        int n = this.vertices.Count;
        this.vertices.Add(supportPoint);

        // Add new faces.
        vertices = Vertices;
        foreach ((int a, int b) in edges) {
            PolytopeFace face = new PolytopeFace(vertices, new int3(a, b, n), default);

            // Skip degenerate faces.
            if (face.Distance != 0)
                faces.Add(face);
        }
        edges.RemoveLastWithoutClear(edges.Count);
    }

    public void CheckTopology() {
        foreach (PolytopeFace face in Faces) {
            float a = Vertices[face.VertexId.X].OriginA.Distance(Vertices[face.VertexId.Y].OriginA);
            float b = Vertices[face.VertexId.Y].OriginA.Distance(Vertices[face.VertexId.Z].OriginA);
            float c = Vertices[face.VertexId.Z].OriginA.Distance(Vertices[face.VertexId.X].OriginA);

            if (a + b <= c || a + c <= b || b + c <= a)
                throw new InvalidOperationException("Invalid topology.");
        }
    }

}
