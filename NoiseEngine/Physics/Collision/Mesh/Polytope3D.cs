using NoiseEngine.Collections;
using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal ref struct Polytope3D {

    private SpanList<SupportPoint> vertices;
    private SpanList<PolytopeFace> faces;

    public readonly Span<SupportPoint> Vertices => vertices.Data;
    public readonly Span<PolytopeFace> Faces => faces.Data;

    public Polytope3D(Span<SupportPoint> vertices, int verticesCount, Span<PolytopeFace> faces, int facesCount) {
        this.vertices = new SpanList<SupportPoint>(vertices, verticesCount);
        this.faces = new SpanList<PolytopeFace>(faces, facesCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveOrAddEdge(ref SpanList<(int, int)> edges, (int, int) edge) {
        for (int i = 0; i < edges.Count; i++) {
            (int a, int b) = edges.buffer[i];
            if (a == edge.Item2 && b == edge.Item1) {
                if (--edges.Count >= 0)
                    edges.buffer[i] = edges.buffer[edges.Count];
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
    public bool TryPop(out int faceId) {
        if (faces.Count == 0) {
            faceId = default;
            return false;
        }

        faceId = --faces.Count;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly (PolytopeFace face, int faceId) ClosestFaceToOrigin() {
        Span<PolytopeFace> faces = Faces;
        PolytopeFace face = faces[0];

        int id = 0;
        for (int i = 1; i < faces.Length; i++) {
            ref PolytopeFace f = ref faces[i];
            if (f.Distance < face.Distance && face.IsDeleted) {
                face = f;
                id = i;
            }
        }

        return (face, id);
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in PolytopeFace face, SupportPoint supportPoint, int faceId) {
        SpanList<(int, int)> edges = new SpanList<(int, int)>(stackalloc (int, int)[Epa.MaxIterations * 9], 0);

        faces.buffer[faceId].IsDeleted = true;

        int3 oppVertexId = face.NextCcw(faces.buffer);
        ComputeEdges(ref edges, faces.buffer, vertices.buffer, in supportPoint, face.NeighborId.X, oppVertexId.X);
        ComputeEdges(ref edges, faces.buffer, vertices.buffer, in supportPoint, face.NeighborId.Y, oppVertexId.Y);
        ComputeEdges(ref edges, faces.buffer, vertices.buffer, in supportPoint, face.NeighborId.Z, oppVertexId.Z);

        Debug.Assert(edges.Count > 0);

        int supportPointId = vertices.Count;
        vertices.Add(supportPoint);

        foreach ((int edgeFaceId, int b) in edges.Data) {
            ref PolytopeFace edgeFace = ref faces.buffer[edgeFaceId];
            if (edgeFace.IsDeleted)
                continue;

            int a = (b + 1) % 3;
            int newFaceId = faces.Count;
            int vertexId1 = edgeFace.VertexId[(b + 2) % 3];
            int vertexId2 = edgeFace.VertexId[a];

            edgeFace.NeighborId = a switch {
                0 => edgeFace.NeighborId with { X = newFaceId },
                1 => edgeFace.NeighborId with { Y = newFaceId },
                2 => edgeFace.NeighborId with { Z = newFaceId },
                _ => throw new ArgumentOutOfRangeException(nameof(a)),
            };

            faces.Add(new PolytopeFace(
                vertices.buffer, new int3(vertexId1, vertexId2, supportPointId),
                new int3(edgeFaceId, newFaceId + 1, newFaceId - 1)
            ));
        }

        /*Span<SupportPoint> vertices = Vertices;
        Span<PolytopeFace> faces = Faces;

        int i = 0;
        while (i < this.faces.Count) {
            PolytopeFace face = faces[i];
            float dot = face.Normal.Dot(supportPoint.Value - vertices[face.VertexId.X].Value);
            if (dot > 0) {
                RemoveOrAddEdge(ref edges, (face.VertexId.X, face.VertexId.Y));
                RemoveOrAddEdge(ref edges, (face.VertexId.Y, face.VertexId.Z));
                RemoveOrAddEdge(ref edges, (face.VertexId.Z, face.VertexId.X));

                if (--this.faces.Count > 0)
                    this.faces.buffer[i] = this.faces.buffer[this.faces.Count];
            } else {
                i++;
            }
        }

        // Add vertex.
        int n = this.vertices.Count;
        this.vertices.Add(supportPoint);

        // Add new faces.
        vertices = Vertices;
        foreach ((int a, int b) in edges.Data)
            this.faces.Add(new PolytopeFace(vertices, new int3(n, a, b)));*/
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
