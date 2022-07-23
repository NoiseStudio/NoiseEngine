using NoiseEngine.Jobs;
using System;
using System.Collections.ObjectModel;

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

        Assert.True(filter.CompareComponents(new ReadOnlyCollection<Type>(new Type[] {
            typeof(int)
        })));
        Assert.True(filter.CompareComponents(new ReadOnlyCollection<Type>(new Type[] {
            typeof(int), typeof(bool)
        })));
        Assert.False(filter.CompareComponents(new ReadOnlyCollection<Type>(new Type[] {
            typeof(int), typeof(string)
        })));
        Assert.False(filter.CompareComponents(new ReadOnlyCollection<Type>(new Type[] {
            typeof(bool)
        })));
    }

}
