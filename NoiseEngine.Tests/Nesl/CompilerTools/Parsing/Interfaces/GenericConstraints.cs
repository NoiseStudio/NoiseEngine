using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class GenericConstraints : NeslParsingTestEnvironment {

    [Fact]
    public void SatisfyFor() {
        CompileSingle(@"
            using System;
            using System.Operators;

            struct Wrapper<T> : IAdd<Wrapper<T>, Wrapper<T>, Wrapper<T>> for T : [IAdd<T, T, T>] {
                public T Value;

                public Wrapper(T value) {
                    Value = value;
                }

                public static Wrapper<T> Add(Wrapper<T> lhs, Wrapper<T> rhs) where T : IAdd<T, T, T> {
                    return new Wrapper<T>(lhs.Value + rhs.Value);
                }
            }

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = (new Wrapper<f32>(420.69) + new Wrapper<f32>(1337.42)).Value;
            }
        ");
    }

    [Fact]
    public void NotSatisfyFor() {
        CompileSingleThrowAny(@"
            using System;
            using System.Operators;

            struct Wrapper<T> : IAdd<Wrapper<T>, Wrapper<T>, Wrapper<T>> for T : [IAdd<T, T, T>] {
                public T Value;

                public Wrapper(T value) {
                    Value = value;
                }

                public static Wrapper<T> Add(Wrapper<T> lhs, Wrapper<T> rhs) where T : IAdd<T, T, T> {
                    return new Wrapper<T>(lhs.Value + rhs.Value);
                }
            }

            struct Mock {
                public f32 Value;

                public Mock(f32 value) {
                    Value = value;
                }
            }

            uniform RwBuffer<Mock> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = (new Wrapper<Mock>(new Mock(420.69)) + new Wrapper<Mock>(new Mock(1337.42))).Value;
            }
        ");
    }

    [Fact]
    public void NotSatisfyForWithoutUsage() {
        CompileSingle(@"
            using System;
            using System.Operators;

            struct Wrapper<T> : IAdd<Wrapper<T>, Wrapper<T>, Wrapper<T>> for T : [IAdd<T, T, T>] {
                public T Value;

                public Wrapper(T value) {
                    Value = value;
                }

                public static Wrapper<T> Add(Wrapper<T> lhs, Wrapper<T> rhs) where T : IAdd<T, T, T> {
                    return new Wrapper<T>(lhs.Value + rhs.Value);
                }
            }

            struct Mock {
                public f32 Value;

                public Mock(f32 value) {
                    Value = value;
                }
            }

            uniform RwBuffer<Mock> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = new Wrapper<Mock>(new Mock(420.69)).Value;
            }
        ");
    }

}
