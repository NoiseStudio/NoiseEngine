using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NoiseEngine.Mathematics.Helpers;

#if NE_LARGE_WORLD
using Inner = double;
#else
using Inner = float;
#endif

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Position :
    IComparable,
    IConvertible,
    IComparable<Position>,
    IEquatable<Position>,
    IBinaryFloatingPointIeee754<Position>,
    IMinMaxValue<Position>
{

    private readonly Inner inner;

    public static Position MaxValue => new Position(Inner.MaxValue);
    public static Position MinValue => new Position(Inner.MinValue);

    public static Position Epsilon => new Position(Inner.Epsilon);
    public static Position NaN => new Position(Inner.NaN);
    public static Position NegativeInfinity => new Position(Inner.NegativeInfinity);
    public static Position NegativeZero => new Position(Inner.NegativeZero);
    public static Position PositiveInfinity => new Position(Inner.PositiveInfinity);
    public static Position E => new Position(Inner.E);
    public static Position Pi => new Position(Inner.Pi);
    public static Position Tau => new Position(Inner.Tau);

    static Position ISignedNumber<Position>.NegativeOne => new Position(-1);
    static Position INumberBase<Position>.One => new Position(1);
    static int INumberBase<Position>.Radix => NumberBaseHelper<Inner>.Radix;
    static Position INumberBase<Position>.Zero => new Position(0);
    static Position IAdditiveIdentity<Position, Position>.AdditiveIdentity =>
        new Position(AdditiveIdentityHelper<Inner, Inner>.AdditiveIdentity);
    static Position IMultiplicativeIdentity<Position, Position>.MultiplicativeIdentity =>
        new Position(MultiplicativeIdentityHelper<Inner, Inner>.MultiplicativeIdentity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Position(Inner inner) {
        this.inner = inner;
    }

    /// <summary>Determines if a value is a power of two.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is a power of two; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPow2(Position value) {
        return BinaryNumberHelper<Inner>.IsPow2(value.inner);
    }

    /// <summary>Computes the log2 of a value.</summary>
    /// <param name="value">The value whose log2 is to be computed.</param>
    /// <returns>The log2 of <paramref name="value" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Log2(Position value) {
        return new Position(BinaryNumberHelper<Inner>.Log2(value.inner));
    }

    /// <summary>Computes the arc-tangent for the quotient of two values.</summary>
    /// <param name="y">The y-coordinate of a point.</param>
    /// <param name="x">The x-coordinate of a point.</param>
    /// <returns>The arc-tangent of <paramref name="y" /> divided-by <paramref name="x" />.</returns>
    /// <remarks>This computes <c>arctan(y / x)</c> in the interval <c>[-π, +π]</c> radians.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Atan2(Position y, Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Atan2(y.inner, x.inner));
    }

    /// <summary>Computes the arc-tangent for the quotient of two values and divides the result by <c>pi</c>.</summary>
    /// <param name="y">The y-coordinate of a point.</param>
    /// <param name="x">The x-coordinate of a point.</param>
    /// <returns>
    /// The arc-tangent of <paramref name="y" /> divided-by <paramref name="x" />, divided by <c>pi</c>.
    /// </returns>
    /// <remarks>This computes <c>arctan(y / x) / π</c> in the interval <c>[-1, +1]</c>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Atan2Pi(Position y, Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Atan2Pi(y.inner, x.inner));
    }

    /// <summary>Decrements a value to the largest value that compares less than a given value.</summary>
    /// <param name="x">The value to be bitwise decremented.</param>
    /// <returns>The largest value that compares less than <paramref name="x" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position BitDecrement(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.BitDecrement(x.inner));
    }

    /// <summary>Increments a value to the smallest value that compares greater than a given value.</summary>
    /// <param name="x">The value to be bitwise incremented.</param>
    /// <returns>The smallest value that compares greater than <paramref name="x" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position BitIncrement(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.BitIncrement(x.inner));
    }

    /// <summary>Computes the fused multiply-add of three values.</summary>
    /// <param name="left">The value which <paramref name="right" /> multiplies.</param>
    /// <param name="right">The value which multiplies <paramref name="left" />.</param>
    /// <param name="addend">
    /// The value that is added to the product of <paramref name="left" /> and <paramref name="right" />.
    /// </param>
    /// <returns>
    /// The result of <paramref name="left" /> times <paramref name="right" /> plus <paramref name="addend" /> computed
    /// as one ternary operation.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position FusedMultiplyAdd(Position left, Position right, Position addend) {
        return new Position(FloatingPointIeee754Helper<Inner>.FusedMultiplyAdd(left.inner, right.inner, addend.inner));
    }

    /// <summary>Computes the integer logarithm of a value.</summary>
    /// <param name="x">The value whose integer logarithm is to be computed.</param>
    /// <returns>The integer logarithm of <paramref name="x" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ILogB(Position x) {
        return FloatingPointIeee754Helper<Inner>.ILogB(x.inner);
    }

    /// <summary>Computes the product of a value and its base-radix raised to the specified power.</summary>
    /// <param name="x">The value which base-radix raised to the power of <paramref name="n" /> multiplies.</param>
    /// <param name="n">The value to which base-radix is raised before multipliying <paramref name="x" />.</param>
    /// <returns>
    /// The product of <paramref name="x" /> and base-radix raised to the power of <paramref name="n" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position ScaleB(Position x, int n) {
        return new Position(FloatingPointIeee754Helper<Inner>.ScaleB(x.inner, n));
    }

    /// <summary>Computes <c>E</c> raised to a given power.</summary>
    /// <param name="x">The power to which <c>E</c> is raised.</param>
    /// <returns><c>E<sup><paramref name="x" /></sup></c></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Exp(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Exp(x.inner));
    }

    /// <summary>Computes <c>10</c> raised to a given power.</summary>
    /// <param name="x">The power to which <c>10</c> is raised.</param>
    /// <returns><c>10<sup><paramref name="x" /></sup></c></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Exp10(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Exp10(x.inner));
    }

    /// <summary>Computes <c>2</c> raised to a given power.</summary>
    /// <param name="x">The power to which <c>2</c> is raised.</param>
    /// <returns><c>2<sup><paramref name="x" /></sup></c></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Exp2(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Exp2(x.inner));
    }

    /// <summary>
    /// Rounds a value to a specified number of fractional-digits using the default rounding mode
    /// (<see cref="MidpointRounding.ToEven" />).
    /// </summary>
    /// <param name="x">The value to round.</param>
    /// <param name="digits">The number of fractional digits to which <paramref name="x" /> should be rounded.</param>
    /// <param name="mode">The mode under which <paramref name="x" /> should be rounded.</param>
    /// <returns>
    /// The result of rounding <paramref name="x" /> to <paramref name="digits" /> fractional-digits using
    /// <paramref name="mode" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Round(Position x, int digits, MidpointRounding mode) {
        return new Position(FloatingPointIeee754Helper<Inner>.Round(x.inner, digits, mode));
    }

    /// <summary>
    /// Computes the hyperbolic arc-cosine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-cosine is to be computed.</param>
    /// <returns>The hyperbolic arc-cosine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Acosh(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Acosh(x.inner));
    }

    /// <summary>
    /// Computes the hyperbolic arc-sine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-sine is to be computed.</param>
    /// <returns>The hyperbolic arc-sine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Asinh(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Asinh(x.inner));
    }

    /// <summary>
    /// Computes the hyperbolic arc-tangent of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose hyperbolic arc-tangent is to be computed.</param>
    /// <returns>The hyperbolic arc-tangent of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Atanh(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Atanh(x.inner));
    }

    /// <summary>
    /// Computes the hyperbolic cosine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose hyperbolic cosine is to be computed.</param>
    /// <returns>The hyperbolic cosine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Cosh(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Cosh(x.inner));
    }

    /// <summary>
    /// Computes the hyperbolic sine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose hyperbolic sine is to be computed.</param>
    /// <returns>The hyperbolic sine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Sinh(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Sinh(x.inner));
    }

    /// <summary>
    /// Computes the hyperbolic tangent of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose hyperbolic tangent is to be computed.</param>
    /// <returns>The hyperbolic tangent of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Tanh(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Tanh(x.inner));
    }

    /// <summary>Computes the natural (<c>base-E</c>) logarithm of a value.</summary>
    /// <param name="x">The value whose natural logarithm is to be computed.</param>
    /// <returns><c>log<sub>e</sub>(<paramref name="x" />)</c></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Log(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Log(x.inner));
    }

    /// <summary>Computes the logarithm of a value in the specified base.</summary>
    /// <param name="x">The value whose logarithm is to be computed.</param>
    /// <param name="newBase">The base in which the logarithm is to be computed.</param>
    /// <returns><c>log<sub><paramref name="newBase" /></sub>(<paramref name="x" />)</c></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Log(Position x, Position newBase) {
        return new Position(FloatingPointIeee754Helper<Inner>.Log(x.inner, newBase.inner));
    }

    /// <summary>Computes the base-10 logarithm of a value.</summary>
    /// <param name="x">The value whose base-10 logarithm is to be computed.</param>
    /// <returns><c>log<sub>10</sub>(<paramref name="x" />)</c></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Log10(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Log10(x.inner));
    }

    /// <summary>
    /// Computes a value raised to a given power.
    /// </summary>
    /// <param name="x">The value which is raised to the power of x.</param>
    /// <param name="y">The power to which x is raised.</param>
    /// <returns>x raised to the power of y.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Pow(Position x, Position y) {
        return new Position(FloatingPointIeee754Helper<Inner>.Pow(x.inner, y.inner));
    }

    /// <summary>
    /// Computes the cube-root of a value.
    /// </summary>
    /// <param name="x">The value whose cube-root is to be computed.</param>
    /// <returns>The cube-root of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Cbrt(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Cbrt(x.inner));
    }

    /// <summary>
    /// Computes the hypotenuse given two values representing the lengths of the shorter sides in a right-angled
    /// triangle.
    /// </summary>
    /// <param name="x">The value to square and add to y.</param>
    /// <param name="y">The value to square and add to x.</param>
    /// <returns>The square root of x-squared plus y-squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Hypot(Position x, Position y) {
        return new Position(FloatingPointIeee754Helper<Inner>.Hypot(x.inner, y.inner));
    }

    /// <summary>
    /// Computes the n-th root of a value.
    /// </summary>
    /// <param name="x">The value whose n-th root is to be computed.</param>
    /// <param name="n">The degree of the root to be computed.</param>
    /// <returns>The n-th root of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position RootN(Position x, int n) {
        return new Position(FloatingPointIeee754Helper<Inner>.RootN(x.inner, n));
    }

    /// <summary>
    /// Computes the square-root of a value.
    /// </summary>
    /// <param name="x">The value whose square-root is to be computed.</param>
    /// <returns>The square-root of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Sqrt(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Sqrt(x.inner));
    }

    /// <summary>
    /// Computes the arc-cosine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose arc-cosine is to be computed.</param>
    /// <returns>The arc-cosine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Acos(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Acos(x.inner));
    }

    /// <summary>
    /// Computes the arc-cosine of a value and divides the result by pi.
    /// </summary>
    /// <param name="x">The value whose arc-cosine is to be computed.</param>
    /// <returns>The arc-cosine of x, divided by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position AcosPi(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.AcosPi(x.inner));
    }

    /// <summary>
    /// Computes the arc-sine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose arc-sine is to be computed.</param>
    /// <returns>The arc-sine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Asin(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Asin(x.inner));
    }

    /// <summary>
    /// Computes the arc-sine of a value and divides the result by pi.
    /// </summary>
    /// <param name="x">The value whose arc-sine is to be computed.</param>
    /// <returns>The arc-sine of x, divided by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position AsinPi(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.AsinPi(x.inner));
    }

    /// <summary>
    /// Computes the arc-tangent of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose arc-tangent is to be computed.</param>
    /// <returns>The arc-tangent of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Atan(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Atan(x.inner));
    }

    /// <summary>
    /// Computes the arc-tangent of a value and divides the result by pi.
    /// </summary>
    /// <param name="x">The value whose arc-tangent is to be computed.</param>
    /// <returns>The arc-tangent of x, divided by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position AtanPi(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.AtanPi(x.inner));
    }

    /// <summary>
    /// Computes the cosine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose cosine is to be computed.</param>
    /// <returns>The cosine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Cos(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Cos(x.inner));
    }

    /// <summary>
    /// Computes the cosine of a value that has been multipled by pi.
    /// </summary>
    /// <param name="x">The value, in half-revolutions, whose cosine is to be computed.</param>
    /// <returns>The cosine of x multiplied-by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position CosPi(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.CosPi(x.inner));
    }

    /// <summary>
    /// Computes the sine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose sine is to be computed.</param>
    /// <returns>The sine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Sin(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Sin(x.inner));
    }

    /// <summary>
    /// Computes the sine and cosine of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose sine and cosine are to be computed.</param>
    /// <returns>The sine and cosine of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Position Sin, Position Cos) SinCos(Position x) {
        (Inner sin, Inner cos) = FloatingPointIeee754Helper<Inner>.SinCos(x.inner);
        return (new Position(sin), new Position(cos));
    }

    /// <summary>
    /// Computes the sine and cosine of a value that has been multiplied by pi.
    /// </summary>
    /// <param name="x">
    /// The value, in half-revolutions, that is multipled by pi before computing its sine and cosine.
    /// </param>
    /// <returns>The sine and cosine ofx multiplied-by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Position SinPi, Position CosPi) SinCosPi(Position x) {
        (Inner sinPi, Inner cosPi) = FloatingPointIeee754Helper<Inner>.SinCosPi(x.inner);
        return (new Position(sinPi), new Position(cosPi));
    }

    /// <summary>
    /// Computes the sine of a value that has been multiplied by pi.
    /// </summary>
    /// <param name="x">The value, in half-revolutions, that is multipled by pi before computing its sine.</param>
    /// <returns>The sine of x multiplied-by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position SinPi(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.SinPi(x.inner));
    }

    /// <summary>
    /// Computes the tangent of a value.
    /// </summary>
    /// <param name="x">The value, in radians, whose tangent is to be computed.</param>
    /// <returns>The tangent of x.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Tan(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.Tan(x.inner));
    }

    /// <summary>
    /// Computes the tangent of a value that has been multipled by pi.
    /// </summary>
    /// <param name="x">The value, in half-revolutions, that is multipled by pi before computing its tangent.</param>
    /// <returns>The tangent of x multiplied-by pi.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position TanPi(Position x) {
        return new Position(FloatingPointIeee754Helper<Inner>.TanPi(x.inner));
    }

    /// <summary>Computes the absolute of a value.</summary>
    /// <param name="value">The value for which to get its absolute.</param>
    /// <returns>The absolute of <paramref name="value" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Abs(Position value) {
        return new Position(FloatingPointIeee754Helper<Inner>.Abs(value.inner));
    }

    /// <summary>Determines if a value represents an even integral value.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is an even integer; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     <para>
    ///     This correctly handles floating-point values and so <c>2.0</c> will return <c>true</c> while <c>2.2</c> will
    ///     return <c>false</c>.
    ///     </para>
    ///     <para>
    ///     This functioning returning <c>false</c> does not imply that <see cref="IsOddInteger(Position)" /> will
    ///     return <c>true</c>. A number with a fractional portion, <c>3.3</c>, is not even nor odd.
    ///     </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEvenInteger(Position value) {
        return NumberBaseHelper<Inner>.IsEvenInteger(value.inner);
    }

    /// <summary>Determines if a value is finite.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is finite; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This function returning <c>false</c> does not imply that <see cref="IsInfinity(Position)" /> will return
    /// <c>true</c>. <c>NaN</c> is not finite nor infinite.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(Position value) {
        return NumberBaseHelper<Inner>.IsFinite(value.inner);
    }

    /// <summary>Determines if a value is infinite.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is infinite; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This function returning <c>false</c> does not imply that <see cref="IsFinite(Position)" /> will return
    /// <c>true</c>. <c>NaN</c> is not finite nor infinite.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInfinity(Position value) {
        return NumberBaseHelper<Inner>.IsInfinity(value.inner);
    }

    /// <summary>Determines if a value represents an integral value.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is an integer; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This correctly handles floating-point values and so <c>2.0</c> and <c>3.0</c> will return <c>true</c> while
    /// <c>2.2</c> and <c>3.3</c> will return <c>false</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(Position value) {
        return NumberBaseHelper<Inner>.IsInteger(value.inner);
    }

    /// <summary>Determines if a value is NaN.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is NaN; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(Position value) {
        return NumberBaseHelper<Inner>.IsNaN(value.inner);
    }

    /// <summary>Determines if a value is negative.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is negative; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This function returning <c>false</c> does not imply that <see cref="IsPositive(Position)" /> will return
    /// <c>true</c>. A complex number, <c>a + bi</c> for non-zero <c>b</c>, is not positive nor negative.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(Position value) {
        return NumberBaseHelper<Inner>.IsNegative(value.inner);
    }

    /// <summary>Determines if a value is negative infinity.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is negative infinity; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegativeInfinity(Position value) {
        return NumberBaseHelper<Inner>.IsNegativeInfinity(value.inner);
    }

    /// <summary>Determines if a value is normal.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is normal; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNormal(Position value) {
        return NumberBaseHelper<Inner>.IsNormal(value.inner);
    }

    /// <summary>Determines if a value represents an odd integral value.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is an odd integer; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     <para>
    ///     This correctly handles floating-point values and so <c>3.0</c> will return <c>true</c> while <c>3.3</c> will
    ///     return <c>false</c>.
    ///     </para>
    ///     <para>
    ///     This functioning returning <c>false</c> does not imply that <see cref="IsOddInteger(Position)" /> will
    ///     return <c>true</c>. A number with a fractional portion, <c>3.3</c>, is neither even nor odd.
    ///     </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOddInteger(Position value) {
        return NumberBaseHelper<Inner>.IsOddInteger(value.inner);
    }

    /// <summary>Determines if a value is positive.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is positive; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This function returning <c>false</c> does not imply that <see cref="IsNegative(Position)" /> will return
    /// <c>true</c>. A complex number, <c>a + bi</c> for non-zero <c>b</c>, is not positive nor negative.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(Position value) {
        return NumberBaseHelper<Inner>.IsPositive(value.inner);
    }

    /// <summary>Determines if a value is positive infinity.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is positive infinity; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositiveInfinity(Position value) {
        return NumberBaseHelper<Inner>.IsPositiveInfinity(value.inner);
    }

    /// <summary>Determines if a value represents a real value.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is a real number; otherwise, <c>false</c>.</returns>
    /// <remarks>This function returns <c>true</c> for a complex number <c>a + bi</c> where <c>b</c> is zero.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRealNumber(Position value) {
        return NumberBaseHelper<Inner>.IsRealNumber(value.inner);
    }

    /// <summary>Determines if a value is subnormal.</summary>
    /// <param name="value">The value to be checked.</param>
    /// <returns><c>true</c> if <paramref name="value" /> is subnormal; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSubnormal(Position value) {
        return NumberBaseHelper<Inner>.IsSubnormal(value.inner);
    }

    /// <summary>Compares two values to compute which is greater.</summary>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns>
    /// <paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.
    /// </returns>
    /// <remarks>
    /// This method matches the IEEE 754:2019 <c>maximumMagnitude</c> function. This requires NaN inputs to be
    /// propagated back to the caller and for <c>-0.0</c> to be treated as less than <c>+0.0</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position MaxMagnitude(Position x, Position y) {
        return new Position(NumberBaseHelper<Inner>.MaxMagnitude(x.inner, y.inner));
    }

    /// <summary>
    /// Compares two values to compute which has the greater magnitude and returning the other value if an input is
    /// <c>NaN</c>.
    /// </summary>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns>
    /// <paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.
    /// </returns>
    /// <remarks>
    /// This method matches the IEEE 754:2019 <c>maximumMagnitudeNumber</c> function. This requires NaN inputs to not be
    /// propagated back to the caller and for <c>-0.0</c> to be treated as less than <c>+0.0</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position MaxMagnitudeNumber(Position x, Position y) {
        return new Position(NumberBaseHelper<Inner>.MaxMagnitudeNumber(x.inner, y.inner));
    }

    /// <summary>Compares two values to compute which is lesser.</summary>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns>
    /// <paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.
    /// </returns>
    /// <remarks>
    /// This method matches the IEEE 754:2019 <c>minimumMagnitude</c> function. This requires NaN inputs to be
    /// propagated back to the caller and for <c>-0.0</c> to be treated as less than <c>+0.0</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position MinMagnitude(Position x, Position y) {
        return new Position(NumberBaseHelper<Inner>.MinMagnitude(x.inner, y.inner));
    }

    /// <summary>
    /// Compares two values to compute which has the lesser magnitude and returning the other value if an input is
    /// <c>NaN</c>.
    /// </summary>
    /// <param name="x">The value to compare with <paramref name="y" />.</param>
    /// <param name="y">The value to compare with <paramref name="x" />.</param>
    /// <returns>
    /// <paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.
    /// </returns>
    /// <remarks>
    /// This method matches the IEEE 754:2019 <c>minimumMagnitudeNumber</c> function. This requires NaN inputs to not be
    /// propagated back to the caller and for <c>-0.0</c> to be treated as less than <c>+0.0</c>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position MinMagnitudeNumber(Position x, Position y) {
        return new Position(NumberBaseHelper<Inner>.MinMagnitudeNumber(x.inner, y.inner));
    }

    /// <summary>Parses a string into a value.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information about <paramref name="s" />.
    /// </param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="s" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException">
    /// <paramref name="s" /> is not representable by <see cref="Position"/>.
    /// </exception>
    public static Position Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) {
        return new Position(Inner.Parse(s, style, provider));
    }

    /// <summary>Parses a span of characters into a value.</summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information about <paramref name="s" />.
    /// </param>
    /// <returns>The result of parsing <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.
    /// </exception>
    /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
    /// <exception cref="OverflowException">
    /// <paramref name="s" /> is not representable by <see cref="Position"/>.
    /// </exception>
    public static Position Parse(string s, NumberStyles style, IFormatProvider? provider) {
        return Parse(s.AsSpan(), style, provider);
    }

    /// <summary>Tries to parses a span of characters into a value.</summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information about <paramref name="s" />.
    /// </param>
    /// <param name="result">
    /// On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.
    /// </param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.
    /// </exception>
    public static bool TryParse(
        ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Position result
    ) {
        if (Inner.TryParse(s, style, provider, out Inner inner)) {
            result = new Position(inner);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>Tries to parses a string into a value.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
    /// <param name="provider">
    /// An object that provides culture-specific formatting information about <paramref name="s" />.
    /// </param>
    /// <param name="result">
    /// On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.
    /// </param>
    /// <returns><c>true</c> if <paramref name="s" /> was successfully parsed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="style" /> is not a supported <see cref="NumberStyles" /> value.
    /// </exception>
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Position result) {
        if (s is null) {
            result = default;
            return false;
        }
        return TryParse(s.AsSpan(), style, provider, out result);
    }

    /// <summary>
    /// Parses a span of characters into a value.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about s.</param>
    /// <returns>The result of parsing s.</returns>
    public static Position Parse(ReadOnlySpan<char> s, IFormatProvider? provider) {
        return new Position(Inner.Parse(s, provider));
    }

    /// <summary>
    /// Tries to parse a span of characters into a value.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about s.</param>
    /// <param name="result">
    /// When this method returns, contains the result of successfully parsing s, or an undefined value on failure.
    /// </param>
    /// <returns>true if s was successfully parsed; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Position result) {
        if (Inner.TryParse(s, provider, out Inner inner)) {
            result = new Position(inner);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Parses a string into a value.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about s.</param>
    /// <returns>The result of parsing s.</returns>
    public static Position Parse(string s, IFormatProvider? provider) {
        return Parse(s.AsSpan(), provider);
    }

    /// <summary>
    /// Tries to parse a string into a value.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="provider">An object that provides culture-specific formatting information about s.</param>
    /// <param name="result">
    /// When this method returns, contains the result of successfully parsing s or an undefined value on failure.
    /// </param>
    /// <returns>true if s was successfully parsed; otherwise, false.</returns>
    public static bool TryParse(
        [NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Position result
    ) {
        if (s is null) {
            result = default;
            return false;
        }
        return TryParse(s.AsSpan(), provider, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Position IFloatingPointIeee754<Position>.Ieee754Remainder(Position left, Position right) {
        return new Position(FloatingPointIeee754Helper<Inner>.Ieee754Remainder(left.inner, right.inner));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Position>.IsCanonical(Position value) {
        return NumberBaseHelper<Inner>.IsCanonical(value.inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Position>.IsComplexNumber(Position value) {
        return NumberBaseHelper<Inner>.IsComplexNumber(value.inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Position>.IsImaginaryNumber(Position value) {
        return NumberBaseHelper<Inner>.IsImaginaryNumber(value.inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<Position>.IsZero(Position value) {
        return NumberBaseHelper<Inner>.IsZero(value.inner);
    }

    static bool INumberBase<Position>.TryConvertFromChecked<TOther>(TOther value, out Position result) {
        throw new NotImplementedException();
    }

    static bool INumberBase<Position>.TryConvertFromSaturating<TOther>(TOther value, out Position result) {
        throw new NotImplementedException();
    }

    static bool INumberBase<Position>.TryConvertFromTruncating<TOther>(TOther value, out Position result) {
        throw new NotImplementedException();
    }

    static bool INumberBase<Position>.TryConvertToChecked<TOther>(Position value, out TOther result) {
        throw new NotImplementedException();
    }

    static bool INumberBase<Position>.TryConvertToSaturating<TOther>(Position value, out TOther result) {
        throw new NotImplementedException();
    }

    static bool INumberBase<Position>.TryConvertToTruncating<TOther>(Position value, out TOther result) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Indicates whether this <see cref="Position"/> and <paramref name="obj"/> are equal.
    /// </summary>
    /// <param name="obj">Object to indicate equality.</param>
    /// <returns>
    /// <see langword="true"/> if obj and this <see cref="Position"/> are represent the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is Position position)
            return this.inner.Equals(position.inner);
        if (obj is Inner inner)
            return this.inner.Equals(inner);
        return false;
    }

    /// <summary>
    /// Indicates whether this <see cref="Position"/> and <paramref name="other"/> are equal.
    /// </summary>
    /// <param name="other"><see cref="Position"/> to indicate equality.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> and this <see cref="Position"/> are represent the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Position other) {
        return inner.Equals(other.inner);
    }

    /// <summary>
    /// Returns hash code of this <see cref="Position"/>.
    /// </summary>
    /// <returns>Hash code of this <see cref="Position"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() {
        return inner.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Position"/>.
    /// </summary>
    /// <returns>String representation of this <see cref="Position"/>.</returns>
    public override string ToString() {
        return inner.ToString();
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Position"/> using <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider"><see cref="IFormatProvider"/> for string representation or null.</param>
    /// <returns>Returns a string representation of this <see cref="Position"/>.</returns>
    public string ToString(IFormatProvider? provider) {
        return inner.ToString(provider);
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Position"/> using <paramref name="format"/>.
    /// </summary>
    /// <param name="format">Format of returned string.</param>
    /// <returns>Returns a string representation of this <see cref="Position"/>.</returns>
    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format) {
        return inner.ToString(format);
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Position"/> using <paramref name="format"/> and
    /// <paramref name="provider"/>.
    /// </summary>
    /// <param name="format">Format of returned string.</param>
    /// <param name="provider"><see cref="IFormatProvider"/> for string representation or null.</param>
    /// <returns>Returns a string representation of this <see cref="Position"/>.</returns>
    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? provider
    ) {
        return inner.ToString(format, provider);
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">
    /// The span in which to write this instance's value formatted as a span of characters.
    /// </param>
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters that were written in destination.
    /// </param>
    /// <param name="format">
    /// A span containing the characters that represent a standard or custom format string that defines the acceptable
    /// format for destination.
    /// </param>
    /// <param name="provider">
    /// An optional object that supplies culture-specific formatting information for destination.
    /// </param>
    /// <returns>true if the formatting was successful; otherwise, false.</returns>
    public bool TryFormat(
        Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default,
        IFormatProvider? provider = null
    ) {
        return inner.TryFormat(destination, out charsWritten, format, provider);
    }

    /// <summary>
    /// Compares this <see cref="Position"/> to another <see cref="Position"/> or <see cref="Inner"/>
    /// returning an integer that indicates the relationship.
    /// </summary>
    /// <param name="obj">Compared <see cref="Position"/> or <see cref="Inner"/>.</param>
    /// <returns>
    /// Returns a value less than zero if this object null is considered to be less than any instance.
    /// </returns>
    public int CompareTo(object? obj) {
        if (obj is Position position)
            return this.inner.CompareTo(position.inner);
        if (obj is Inner inner)
            return this.inner.CompareTo(inner);
        throw new ArgumentException("Object must be of type Position or Inner.", nameof(obj));
    }

    /// <summary>
    /// Compares this <see cref="Position"/> to another <see cref="Position"/> returning an integer that indicates the
    /// relationship.
    /// </summary>
    /// <param name="value">Compared <see cref="Position"/>.</param>
    /// <returns>
    /// Returns a value less than zero if this object null is considered to be less than any instance.
    /// </returns>
    public int CompareTo(Position value) {
        return inner.CompareTo(value.inner);
    }

    #region IConvertible

    /// <summary>
    /// Returns <see cref="TypeCode"/> of this type.
    /// </summary>
    /// <returns>Returns <see cref="TypeCode"/> of this type.</returns>
    public TypeCode GetTypeCode() {
        return inner.GetTypeCode();
    }

    bool IConvertible.ToBoolean(IFormatProvider? provider) {
        return Convert.ToBoolean(inner);
    }

    byte IConvertible.ToByte(IFormatProvider? provider) {
        return Convert.ToByte(inner);
    }

    char IConvertible.ToChar(IFormatProvider? provider) {
        throw new InvalidCastException($"Unable to convert {nameof(Position)} to char.");
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) {
        throw new InvalidCastException($"Unable to convert {nameof(Position)} to {nameof(DateTime)}.");
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider) {
        return Convert.ToDecimal(inner);
    }

    double IConvertible.ToDouble(IFormatProvider? provider) {
#if NE_LARGE_WORLD
        return inner;
#else
        return Convert.ToDouble(inner);
#endif
    }

    short IConvertible.ToInt16(IFormatProvider? provider) {
        return Convert.ToInt16(inner);
    }

    int IConvertible.ToInt32(IFormatProvider? provider) {
        return Convert.ToInt32(inner);
    }

    long IConvertible.ToInt64(IFormatProvider? provider) {
        return Convert.ToInt64(inner);
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider) {
        return Convert.ToSByte(inner);
    }

    float IConvertible.ToSingle(IFormatProvider? provider) {
#if NE_LARGE_WORLD
        return Convert.ToSingle(inner);
#else
        return inner;
#endif
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider) {
        return ((IConvertible)this).ToType(conversionType, provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider) {
        return Convert.ToUInt16(provider);
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider) {
        return Convert.ToUInt32(provider);
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider) {
        return Convert.ToUInt64(provider);
    }

    #endregion IConvertible

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IFloatingPoint<Position>.GetExponentByteCount() {
        return FloatingPointHelper<Inner>.GetExponentByteCount(inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IFloatingPoint<Position>.GetExponentShortestBitLength() {
        return FloatingPointHelper<Inner>.GetExponentShortestBitLength(inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IFloatingPoint<Position>.GetSignificandBitLength() {
        return FloatingPointHelper<Inner>.GetSignificandBitLength(inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IFloatingPoint<Position>.GetSignificandByteCount() {
        return FloatingPointHelper<Inner>.GetSignificandByteCount(inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IFloatingPoint<Position>.TryWriteExponentBigEndian(Span<byte> destination, out int bytesWritten) {
        return FloatingPointHelper<Inner>.TryWriteExponentBigEndian(inner, destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IFloatingPoint<Position>.TryWriteExponentLittleEndian(Span<byte> destination, out int bytesWritten) {
        return FloatingPointHelper<Inner>.TryWriteExponentLittleEndian(inner, destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IFloatingPoint<Position>.TryWriteSignificandBigEndian(Span<byte> destination, out int bytesWritten) {
        return FloatingPointHelper<Inner>.TryWriteSignificandBigEndian(inner, destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IFloatingPoint<Position>.TryWriteSignificandLittleEndian(Span<byte> destination, out int bytesWritten) {
        return FloatingPointHelper<Inner>.TryWriteSignificandLittleEndian(inner, destination, out bytesWritten);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(byte value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(sbyte value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(int value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(uint value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(nint value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(nuint value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(long value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(ulong value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(short value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(ushort value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Position(float value) {
        return new Position(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Position(double value) {
        return new Position((Inner)value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Position(decimal value) {
        return new Position((Inner)value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Position"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Position(Half value) {
        return new Position((Inner)value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="byte"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator byte(Position value) {
        return (byte)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="sbyte"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator sbyte(Position value) {
        return (sbyte)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="int"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator int(Position value) {
        return (int)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="uint"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(Position value) {
        return (uint)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="nint"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator nint(Position value) {
        return (nint)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="nuint"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator nuint(Position value) {
        return (nuint)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="long"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator long(Position value) {
        return (long)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="ulong"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(Position value) {
        return (ulong)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="short"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator short(Position value) {
        return (short)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="ushort"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ushort(Position value) {
        return (ushort)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="float"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator float(Position value) {
        return (float)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="double"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator double(Position value) {
        return (double)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="decimal"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator decimal(Position value) {
        return (decimal)value.inner;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <see cref="Half"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Half(Position value) {
        return (Half)value.inner;
    }

    /// <summary>
    /// Compares two values to determine which is less.
    /// </summary>
    /// <param name="left">The value to compare with right.</param>
    /// <param name="right">The value to compare with left.</param>
    /// <returns>true if left is less than right; otherwise, false.</returns>
    public static bool operator <(Position left, Position right) {
        return left.inner < right.inner;
    }

    /// <summary>
    /// Compares two values to determine which is greater.
    /// </summary>
    /// <param name="left">The value to compare with right.</param>
    /// <param name="right">The value to compare with left.</param>
    /// <returns>true if left is greater than right; otherwise, false.</returns>
    public static bool operator <=(Position left, Position right) {
        return left.inner <= right.inner;
    }

    /// <summary>
    /// Compares two values to determine which is less or equal.
    /// </summary>
    /// <param name="left">The value to compare with right.</param>
    /// <param name="right">The value to compare with left.</param>
    /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
    public static bool operator >(Position left, Position right) {
        return left.inner > right.inner;
    }

    /// <summary>
    /// Compares two values to determine which is greater or equal.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
    public static bool operator >=(Position left, Position right) {
        return left.inner >= right.inner;
    }

    /// <summary>
    /// Divides two values together to compute their modulus or remainder.
    /// </summary>
    /// <param name="left">The value which right divides.</param>
    /// <param name="right">The value which divides left.</param>
    /// <returns>The modulus or remainder of left divided by right.</returns>
    public static Position operator %(Position left, Position right) {
        return new Position(left.inner % right.inner);
    }

    /// <summary>
    /// Adds two values together to compute their sum.
    /// </summary>
    /// <param name="left">The value to which <paramref name="right" /> is added.</param>
    /// <param name="right">The value which is added to <paramref name="left" />.</param>
    /// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
    public static Position operator +(Position left, Position right) {
        return new Position(left.inner + right.inner);
    }

    /// <summary>
    /// Decrements a value.
    /// </summary>
    /// <param name="value">The value to decrement.</param>
    /// <returns>The result of decrementing <paramref name="value" />.</returns>
    public static Position operator --(Position value) {
        return new Position(value.inner - 1);
    }

    /// <summary>
    /// Divides two values together to compute their quotient.
    /// </summary>
    /// <param name="left">The value which <paramref name="right" /> divides.</param>
    /// <param name="right">The value which divides <paramref name="left" />.</param>
    /// <returns>The quotient of <paramref name="left" /> divided-by <paramref name="right" />.</returns>
    public static Position operator /(Position left, Position right) {
        return new Position(left.inner / right.inner);
    }

    /// <summary>
    /// Compares two values to determine equality.
    /// </summary>
    /// <param name="left">The value to compare with right.</param>
    /// <param name="right">The value to compare with left.</param>
    /// <returns>true if left is equal to right; otherwise, false.</returns>
    public static bool operator ==(Position left, Position right) {
        return left.inner == right.inner;
    }

    /// <summary>
    /// Compares two values to determine inequality.
    /// </summary>
    /// <param name="left">The value to compare with right.</param>
    /// <param name="right">The value to compare with left.</param>
    /// <returns>true if left is not equal to right; otherwise, false.</returns>
    public static bool operator !=(Position left, Position right) {
        return left.inner != right.inner;
    }

    /// <summary>
    /// Increments a value.
    /// </summary>
    /// <param name="value">The value to increment.</param>
    /// <returns>The result of incrementing value.</returns>
    public static Position operator ++(Position value) {
        return new Position(value.inner + 1);
    }

    /// <summary>
    /// Multiplies two values together to compute their product.
    /// </summary>
    /// <param name="left">The value which right multiplies.</param>
    /// <param name="right">The value which multiplies left.</param>
    /// <returns>The product of left divided by right.</returns>
    public static Position operator *(Position left, Position right) {
        return new Position(left.inner * right.inner);
    }

    /// <summary>
    /// Subtracts two values to compute their difference.
    /// </summary>
    /// <param name="left">The value from which <paramref name="right" /> is subtracted.</param>
    /// <param name="right">The value which is subtracted from <paramref name="left" />.</param>
    /// <returns>The difference of <paramref name="right" /> subtracted from <paramref name="left" />.</returns>
    public static Position operator -(Position left, Position right) {
        return new Position(left.inner - right.inner);
    }

    /// <summary>
    /// Computes the unary negation of a value.
    /// </summary>
    /// <param name="value">The value for which to compute its unary negation.</param>
    /// <returns>The unary negation of <paramref name="value" />.</returns>
    public static Position operator -(Position value) {
        return new Position(-value.inner);
    }

    /// <summary>
    /// Computes the unary plus of a value.
    /// </summary>
    /// <param name="value">The value for which to compute the unary plus.</param>
    /// <returns>The unary plus of value.</returns>
    public static Position operator +(Position value) {
        return new Position(+value.inner);
    }

    static Position IBitwiseOperators<Position, Position, Position>.operator &(Position left, Position right) {
        return new Position(BitwiseOperatorsHelper<Inner, Inner, Inner>.BitwiseAnd(left.inner, right.inner));
    }

    static Position IBitwiseOperators<Position, Position, Position>.operator |(Position left, Position right) {
        return new Position(BitwiseOperatorsHelper<Inner, Inner, Inner>.BitwiseOr(left.inner, right.inner));
    }

    static Position IBitwiseOperators<Position, Position, Position>.operator ^(Position left, Position right) {
        return new Position(BitwiseOperatorsHelper<Inner, Inner, Inner>.ExclusiveOr(left.inner, right.inner));
    }

    static Position IBitwiseOperators<Position, Position, Position>.operator ~(Position value) {
        return new Position(BitwiseOperatorsHelper<Inner, Inner, Inner>.OnesComplement(value.inner));
    }

}
