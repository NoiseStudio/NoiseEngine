using NoiseEngine.Mathematics;
using System;
using System.Numerics;

namespace NoiseEngine.Tests.Mathematics;

public class Matrix3x3Test {

    [Theory]
    [InlineData(
        new float[] { 65, 55, 54 },
        new float[] {
            -0.0183499f, 0.9998223f, 0.0043115f,
            0.0123642f, 0.0045388f, -0.9999133f,
            -0.9997551f, -0.0182950f, -0.0124453f
        }
    )]
    public void Rotate(float[] init, float[] expected) {
        Quaternion<float> quaternion = NoiseEngine.Mathematics.Quaternion.EulerRadians(Vector3FromSpan<float>(init));
        Assert.Equal(FromSpan<float>(expected).Transpose(), Matrix3x3<float>.Rotate(quaternion));
    }

    [Theory]
    [InlineData(new float[] {
        1, 4, 6,
        8, 9, 7,
        5, 3, 2
    }, new float[] {
        1, 8, 5,
        4, 9, 3,
        6, 7, 2
    })]
    public void Transpose(float[] init, float[] expected) {
        Matrix3x3<float> matrix = FromSpan<float>(init);
        Assert.Equal(FromSpan<float>(expected), matrix.Transpose());
    }

    [Theory]
    [InlineData(new float[] { 2, 2, 1, -3, 0, 4, 1, -1, 5 }, 49)]
    [InlineData(new float[] { 4, 6, 5, 2, 1, 3, 7, 9, 8 }, 9)]
    public void Determinant(float[] init, float expected) {
        Matrix3x3<float> matrix = FromSpan<float>(init);
        Assert.Equal(expected, matrix.Determinant());
    }

    [Theory]
    [InlineData(
        new float[] { 1, 2, -1, 2, 1, 2, -1, 2, 1 },
        new float[] { 3f / 16f, 1f / 4f, -5f / 16f, 1f / 4f, 0f, 1f / 4f, -5f / 16f, 1f / 4f, 3 / 16f }
    )]
    [InlineData(
        new float[] { 4, 6, 5, 2, 1, 3, 7, 9, 8 },
        new float[] { -19f / 9f, -1f / 3f, 13f / 9f, 5f / 9f, -1f / 3f, -2f / 9f, 11f / 9f, 2f / 3f, -8f / 9f }
    )]
    public void TryInvert(float[] init, float[]? expected) {
        Matrix3x3<float> matrix = FromSpan<float>(init);
        if (expected is null) {
            Assert.False(matrix.TryInvert(out _));
        } else {
            Assert.True(matrix.TryInvert(out Matrix3x3<float> invertedMatrix));
            Assert.Equal(FromSpan<float>(expected), invertedMatrix);
        }
    }

    [Theory]
    [InlineData(
        new float[] { 3, 7, 12, 5, 8, 15, 6, 9, 12 },
        new float[] { 1, 4, 7 },
        new float[] { 65, 102, 156 }
    )]
    public void MultipleByVector3(float[] init, float[] vector, float[] expected) {
        Matrix3x3<float> matrix = FromSpan<float>(init);
        Assert.Equal(Vector3FromSpan<float>(expected), matrix * Vector3FromSpan<float>(vector));
    }

    private Matrix3x3<T> FromSpan<T>(ReadOnlySpan<T> data) where T : INumber<T> {
        return new Matrix3x3<T>(
            new Vector3<T>(data[0], data[1], data[2]),
            new Vector3<T>(data[3], data[4], data[5]),
            new Vector3<T>(data[6], data[7], data[8])
        );
    }

    private Vector3<T> Vector3FromSpan<T>(ReadOnlySpan<T> data) where T : INumber<T> {
        return new Vector3<T>(data[0], data[1], data[2]);
    }

}
