using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class JobTest {

        [Fact]
        public void GetHashCodeTest() {
            Job a = new Job(11, GetHashCodeTest);
            Job b = new Job(11, GetHashCodeTest);
            Job c = new Job(69, GetHashCodeTest);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.NotEqual(a.GetHashCode(), c.GetHashCode());
        }

        [Fact]
        public void EqualsTest() {
            Job a = new Job(420, EqualsTest);
            Job b = new Job(420, EqualsTest);
            Job c = new Job(2137, EqualsTest);

            Assert.True(a.Equals((object)b));
            Assert.False(a.Equals((object)c));
            Assert.False(b.Equals(null));
            Assert.False(c.Equals((ulong)2137));

            Assert.True(a == b);
            Assert.False(a == c);
            Assert.True(a != c);
        }

        [Fact]
        public void EqualsGenericTest() {
            Job a = new Job(36, EqualsGenericTest);
            Job b = new Job(36, EqualsGenericTest);
            Job c = new Job(773, EqualsGenericTest);

            Assert.True(a.Equals(b));
            Assert.False(a.Equals(c));
        }

    }
}
