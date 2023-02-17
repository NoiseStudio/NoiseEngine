namespace NoiseEngine.Jobs2.Commands;

internal enum SystemCommandType {
    GetEntity,
    EntityInsert,
    EntityRemove,
    EntityWhen,
    ConditionalEntityContains,
    ConditionalEntityOr,
    ConditionalEntityThen,
    PostConditionalEntityElse,
}
