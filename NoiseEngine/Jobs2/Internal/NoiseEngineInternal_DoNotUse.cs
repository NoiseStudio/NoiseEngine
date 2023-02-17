using System;

namespace NoiseEngine.Jobs2.Internal;

[Obsolete("This class is internal and is not part of the API. Do not use.")]
public static class NoiseEngineInternal_DoNotUse {

    public interface INormalEntitySystem {
    }

    public interface IAffectiveEntitySystem {

        public static abstract Type[] AffectiveComponents { get; }

    }

}
