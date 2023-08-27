using NoiseEngine.Physics.Collision.Mesh;

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
            simplex, data.Pos12, data.HullA, data.ScaleA, data.HullB, data.ScaleB, data.VerticesA, data.VerticesB
        );

        Assert.Equal(data.EpaPosition, result.Position);
        Assert.Equal(data.EpaNormal, result.Normal);
        Assert.Equal(data.EpaDepth, result.Depth);
    }

}
