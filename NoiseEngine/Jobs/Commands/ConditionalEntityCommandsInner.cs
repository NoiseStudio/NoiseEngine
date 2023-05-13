namespace NoiseEngine.Jobs2.Commands;

internal struct ConditionalEntityCommandsInner {

    public readonly Entity Entity { get; }
    public bool UsedOr { get; set; }

    public ConditionalEntityCommandsInner(Entity entity) {
        Entity = entity;
    }

}
