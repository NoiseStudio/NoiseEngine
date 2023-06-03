using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Execute.Methods.Call;

public class GenericConstraints : NeslExecuteTestEnvironment {

    public GenericConstraints(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void CallMethodFromConstraints() {
        RunKernelWithSingleBuffer(@"
            using System;

            interface IFoo {
                f32 Get();
            }

            struct Bar : IFoo {
                public f32 Get() {
                    return 21.37;
                }
            }

            struct Runner<T> where T : IFoo {
                f32 Run(T a) {
                    return a.Get();
                }
            }

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = new Runner<Bar>().Run(new Bar());
            }
        ", null, 21.37f);
    }

    [Fact]
    public void UseOperators() {
        RunKernelWithSingleBuffer(@"
            using System;
            using System.Operators;

            struct Wrapper<T> : IAdd<Wrapper<T>, Wrapper<T>, Wrapper<T>> where T : IAdd<T, T, T> {
                public T Value;

                public Wrapper(T value) {
                    Value = value;
                }

                public static Wrapper<T> Add(Wrapper<T> lhs, Wrapper<T> rhs) {
                    return new Wrapper<T>(lhs.Value + rhs.Value);
                }
            }

            uniform RwBuffer<f32> buffer;

            [Kernel(1, 1, 1)]
            void Main() {
                buffer[0] = 3.0;//new Wrapper<f32>(420.69) + new Wrapper<f32>(1337.42);
            }
        ", null, 1758.11f);
    }

}
