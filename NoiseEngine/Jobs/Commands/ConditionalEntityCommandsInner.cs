namespace NoiseEngine.Jobs.Commands;

internal struct ConditionalEntityCommandsInner {

    public readonly Entity Entity { get; }
    public bool UsedOr { get; set; }

    public ConditionalEntityCommandsInner(Entity entity) {
        Entity = entity;
    }

}
