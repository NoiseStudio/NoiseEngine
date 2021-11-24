using Xunit;
using System.Threading;

namespace NoiseStudio.JobsAg.Tests {
    public class EntityScheduleTest {

        [Fact]
        public void Test1() {
            EntitySchedule schedule = new EntitySchedule();
            EntityWorld world = new EntityWorld();

            TestSystemScheduleA system = new TestSystemScheduleA();
            world.AddSystem(system, 0);

            int entities = 1024;
            for (int i = 0; i < entities; i++)
                world.NewEntity(new TestComponentA(), new TestComponentB());
            while (system.UpdateEntityCount < entities) ;

            while (system.UpdateCount < 2) ;
            Assert.True(system.UsedUpdate);

            for (int i = 0; i < entities; i++)
                world.NewEntity(new TestComponentA(), new TestComponentB());
            while (system.UpdateEntityCount < entities * 2) ;

            Assert.True(system.UpdateCount >= 3);

            Thread.Sleep(50);
            Assert.True(entities * 2 <= system.UpdateEntityCount);
        }

    }
}
