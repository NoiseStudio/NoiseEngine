using NoiseEngine.Collections;
using NoiseEngine.Mathematics;
using NoiseEngine.Physics.Collision.Mesh;
using System.Numerics;
using System.Transactions;

namespace NoiseEngine.Tests.Physics.Collision.Mesh;

public class EpaTest {

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Process(int dataIndex) {
        GjkTestData data = GjkTestData.GetData(dataIndex);
        bool r = Gjk.Intersect(
            data.Pos12, data.HullA, data.ScaleA, data.HullB, data.ScaleB, data.VerticesA, data.VerticesB,
            out Simplex3D simplex
        );

        Assert.Equal(data.Result, r);
        if (!r)
            return;

        EpaResult result = Epa.Process(
            simplex, data.Pos12, data.HullA, data.ScaleA, data.HullB, data.ScaleB, data.VerticesA, data.VerticesB,
            new Polytope3DBuffer(new FastList<PolytopeFace>(), new FastList<(int, int)>())
        );

        Assert.Equal(data.EpaPosition, result.PositionA);
        Assert.Equal(data.EpaNormal, result.Normal);
        Assert.Equal(data.EpaDepth, result.Depth);
    }

    [Fact]
    public void ToWorldSpace() {
        EpaResult result = new EpaResult {
            PositionA = new float3(0.0024874844f, -0.5f, -0.4999876f),
            PositionB = new float3(0.002504078f, -0.45783997f, -0.49997637f),
            Normal = new float3(0, -1, 0),
            Depth = 0.042160038f
        };

        Isometry3<float> posA = new Isometry3<float>(new float3(0f, -4.542162f, 0f), Quaternion<float>.Identity);
        Isometry3<float> posB = new Isometry3<float>(new float3(0.5f, -105f, 0f), Quaternion<float>.Identity);

        var a = posA * result.PositionA;
        var b = posA * result.PositionB;

        var distance = a.Distance(b);
    }

}
