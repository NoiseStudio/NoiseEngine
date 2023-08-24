using NoiseEngine.Physics.Collision.Mesh;

namespace NoiseEngine.Tests.Physics.Collision.Mesh;

public class GjkTest {

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Intersect(int dataIndex) {
        GjkTestData data = GjkTestData.GetData(dataIndex);
        bool result = Gjk.Intersect(
            data.OffsetA, data.HullA, data.ScaleA, data.HullB, data.ScaleB, data.VerticesA, data.VerticesB,
            out Simplex3D simplex
        );

        Assert.Equal(data.Result, result);
        if (!result)
            return;

        Assert.Equal(data.ResultSimplex.A, simplex.A);
        Assert.Equal(data.ResultSimplex.B, simplex.B);
        Assert.Equal(data.ResultSimplex.C, simplex.C);
        Assert.Equal(data.ResultSimplex.D, simplex.D);
    }

}
