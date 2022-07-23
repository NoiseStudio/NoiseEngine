using NoiseEngine.Mathematics;

namespace NoiseEngine.Tests.Mathematics;

public class Matrix4x4Test {

    [Fact]
    public void Scale() {
        Matrix4x4<float> a = new Matrix4x4<float>(
            new Vector4<float>(2, 0, 0, 0),
            new Vector4<float>(0, 3, 0, 0),
            new Vector4<float>(0, 0, 4, 0),
            new Vector4<float>(0, 0, 0, 1)
        );

        Assert.Equal(a, Matrix4x4<float>.Scale(new Vector3<float>(2, 3, 4)));
    }

    [Fact]
    public void Translate() {
        Matrix4x4<float> a = new Matrix4x4<float>(
            new Vector4<float>(1, 0, 0, 0),
            new Vector4<float>(0, 1, 0, 0),
            new Vector4<float>(0, 0, 1, 0),
            new Vector4<float>(2, 3, 4, 1)
        );

        Assert.Equal(a, Matrix4x4<float>.Translate(new Vector3<float>(2, 3, 4)));
    }

    [Fact]
    public void Rotate() {
        Matrix4x4<float> a = new Matrix4x4<float>(
            new Vector4<float>(-49, 10, 20, 0),
            new Vector4<float>(-22, -33, -20, 0),
            new Vector4<float>(-4, -28, -19, 0),
            new Vector4<float>(0, 0, 0, 1)
        );

        Assert.Equal(a, Matrix4x4<float>.Rotate(new Quaternion<float>(1, -3, 4, 2)));
    }

    [Fact]
    public void MultiplyPoint() {
        Matrix4x4<float> a = new Matrix4x4<float>(
            new Vector4<float>(2, 2, 2, 2),
            new Vector4<float>(4, 4, 4, 4),
            new Vector4<float>(2, 2, 2, 2),
            new Vector4<float>(8, 8, 8, 8)
        );

        Assert.Equal(Vector3<float>.One, a.MultiplyPoint(new Vector3<float>(2, 4, 4)));
    }

    [Fact]
    public void OperatorMultiplication() {
        Matrix4x4<float> a = new Matrix4x4<float>(
            new Vector4<float>(51, 43, 51, 51),
            new Vector4<float>(51, 47, 48, 50),
            new Vector4<float>(56, 46, 47, 47),
            new Vector4<float>(49, 45, 51, 51)
        );
        Matrix4x4<float> b = new Matrix4x4<float>(
            new Vector4<float>(2, 3, 4, 5),
            new Vector4<float>(5, 4, 3, 2),
            new Vector4<float>(3, 2, 5, 4),
            new Vector4<float>(5, 4, 2, 3)
        );
        Matrix4x4<float> c = new Matrix4x4<float>(
            new Vector4<float>(3, 2, 5, 4),
            new Vector4<float>(5, 4, 2, 3),
            new Vector4<float>(2, 3, 4, 5),
            new Vector4<float>(5, 4, 3, 2)
        );

        Assert.Equal(a, b * c);
    }

    [Fact]
    public void OperatorMultiplicationByVector() {
        Matrix4x4<float> a = new Matrix4x4<float>(
            new Vector4<float>(2, 3, 4, 5),
            new Vector4<float>(5, 4, 3, 2),
            new Vector4<float>(3, 2, 5, 4),
            new Vector4<float>(5, 4, 2, 3)
        );

        Assert.Equal(new Vector4<float>(85, 69, 81, 73), a * new Vector4<float>(3, 7, 8, 4));
    }

}
