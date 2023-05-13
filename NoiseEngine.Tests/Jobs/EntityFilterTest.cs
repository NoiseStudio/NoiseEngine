using NoiseEngine.Jobs;
using System;

namespace NoiseEngine.Tests.Jobs;

public class EntityFilterTest {

    [Fact]
    public void CompareComponents() {
        EntityFilter filter = new EntityFilter(
            new Type[] {
                typeof(int)
            },
            new Type[] {
                typeof(string)
            }
        );

        Assert.True(filter.CompareComponents(new ComponentType[] {
            new ComponentType(typeof(int), 0)
        }));
        Assert.True(filter.CompareComponents(new ComponentType[] {
            new ComponentType(typeof(int), 0),
            new ComponentType(typeof(bool), 0)
        }));
        Assert.False(filter.CompareComponents(new ComponentType[] {
            new ComponentType(typeof(int), 0),
            new ComponentType(typeof(string), 0)
        }));
        Assert.False(filter.CompareComponents(new ComponentType[] {
            new ComponentType(typeof(bool), 0)
        }));
    }

}
