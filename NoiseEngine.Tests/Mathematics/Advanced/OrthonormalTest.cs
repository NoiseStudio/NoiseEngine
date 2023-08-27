using NoiseEngine.Mathematics.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Mathematics.Advanced;

public class OrthonormalTest {

    [Fact]
    public void SubspaceBasis() {
        int i = 0;
        Orthonormal.SubspaceBasis(new float3(-2, 0, 0), v => {
            switch (i++) {
                case 0:
                    Assert.Equal(new float3(0, -2, 0), v);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return false;
        });

        Assert.Equal(1, i);
    }

}
