namespace NoiseEngine.Jobs.Commands;

internal enum SystemCommandType {
    GetEntity,
    EntityDespawn,
    EntityInsert,
    EntityRemove,
    EntityWhen,
    ConditionalEntityContains,
    ConditionalEntityOr,
    ConditionalEntityThen,
    PostConditionalEntityElse,
}
