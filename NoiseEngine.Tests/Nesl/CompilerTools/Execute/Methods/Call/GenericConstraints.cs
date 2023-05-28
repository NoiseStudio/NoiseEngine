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

}
