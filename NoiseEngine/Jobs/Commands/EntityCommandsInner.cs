using System;

namespace NoiseEngine.Jobs.Commands;

internal sealed class EntityCommandsInner {

    public ConditionalEntityCommandsInner[] Conditionals = Array.Empty<ConditionalEntityCommandsInner>();
    public int ConditionalsCount;

    public Entity Entity { get; }

    public EntityCommandsInner(Entity entity) {
        Entity = entity;
    }

}
