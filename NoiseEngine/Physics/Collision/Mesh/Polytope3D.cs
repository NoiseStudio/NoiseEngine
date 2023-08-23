using NoiseEngine.Collections;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics.Collision.Mesh;

internal ref struct Polytope3D {

    private SpanList<SupportPoint> vertices;
    private SpanList<PolytopeFace> faces;

    public readonly Span<SupportPoint> Vertices => vertices.Data;
    public readonly Span<PolytopeFace> Faces => faces.Data;

    public Polytope3D(Span<SupportPoint> vertices, Span<PolytopeFace> faces) {
        faces[0] = new PolytopeFace(vertices, 3, 2, 1);
        faces[1] = new PolytopeFace(vertices, 3, 1, 0);
        faces[2] = new PolytopeFace(vertices, 3, 0, 2);
        faces[3] = new PolytopeFace(vertices, 2, 0, 1);

        this.vertices = new SpanList<SupportPoint>(vertices, 4);
        this.faces = new SpanList<PolytopeFace>(faces, 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveOrAddEdge(ref SpanList<(int, int)> edges, (int, int) edge) {
        Span<(int, int)> e = edges.Data;
        for (int i = 0; i < e.Length; i++) {
            (int a, int b) = e[i];
            if (a == edge.Item2 && b == edge.Item1) {
                edges.RemoveAt(i);
                return;
            }
        }

        edges.Add(edge);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PolytopeFace ClosestFaceToOrigin() {
        Span<PolytopeFace> faces = Faces;
        PolytopeFace face = faces[0];
        foreach (PolytopeFace f in faces[1..]) {
            if (f.Distance > face.Distance)
                face = f;
        }
        return face;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(SupportPoint supportPoint) {
        SpanList<(int, int)> edges = new SpanList<(int, int)>(stackalloc (int, int)[Epa.MaxIterations * 9], 0);
        Span<SupportPoint> vertices = Vertices;
        Span<PolytopeFace> faces = Faces;

        int i = 0;
        while (i < this.faces.Count) {
            PolytopeFace face = faces[i];
            float dot = face.Normal.Dot(supportPoint.Value - vertices[face.VertexIdA].Value);
            if (dot > 0) {
                this.faces.RemoveAt(i);
                RemoveOrAddEdge(ref edges, (face.VertexIdA, face.VertexIdB));
                RemoveOrAddEdge(ref edges, (face.VertexIdB, face.VertexIdC));
                RemoveOrAddEdge(ref edges, (face.VertexIdC, face.VertexIdA));
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
            this.faces.Add(new PolytopeFace(vertices, n, a, b));
    }

}
