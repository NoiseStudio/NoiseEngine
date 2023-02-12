using NoiseEngine.Collections;
using NoiseEngine.Jobs2.Commands;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public readonly ref struct SystemCommands {

    internal FastList<SystemCommand> Objects { get; }

    internal SystemCommands(FastList<SystemCommand> objects) {
        Objects = objects;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityCommands GetEntity(Entity entity) {
        return new EntityCommands(this, entity);
    }

}
