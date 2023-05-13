namespace NoiseEngine.Jobs2.Commands;

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
