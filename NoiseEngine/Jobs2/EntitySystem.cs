using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public abstract class EntitySystem {

    #region NoiseEngineInternal

    [Obsolete("This class is internal and is not part of the API. Do not use.")]
    protected struct NoiseEngineInternal_DoNotUse {

        public readonly record struct ExecutionData(nint RecordSize, nint StartIndex, nint EndIndex) {

            private readonly Dictionary<Type, nint> offsets;

            internal ExecutionData(
                ArchetypeChunk chunk, nint startIndex, nint endIndex
            ) : this(chunk.RecordSize, startIndex, endIndex) {
                offsets = chunk.Offsets;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public nint GetOffset<T>() where T : IComponent {
                return offsets[typeof(T)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe ref T Get<T>(nint index) where T : IComponent {
                return ref Unsafe.AsRef<T>((byte*)index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Entity? GetInternalComponent(nint index) {
                return Get<EntityInternalComponent>(index).Entity;
            }

        }

        public readonly record struct ComponentUsage(Type Type, bool WriteAccess);

        public ComponentUsage[] UsedComponents { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T NullRef<T>() {
            return ref Unsafe.AsRef<T>((void*)0);
        }

    }

    [Obsolete("This field is internal and is not part of the API. Do not use.")]
    protected NoiseEngineInternal_DoNotUse NoiseEngineInternal_DoNotUse_Storage;

    [Obsolete("This method is internal and is not part of the API. Do not use.")]
    protected virtual void NoiseEngineInternal_DoNotUse_Initialize() {
        throw new InvalidOperationException("NoiseEngine.Generator did not generate the code.");
    }

    [Obsolete("This method is internal and is not part of the API. Do not use.")]
    protected virtual void NoiseEngineInternal_DoNotUse_SystemExecution(
        NoiseEngineInternal_DoNotUse.ExecutionData data
    ) {
        throw new InvalidOperationException("NoiseEngine.Generator did not generate the code.");
    }

    #endregion

    /// <summary>
    /// This method is executed when this system is initializing.
    /// </summary>
    protected virtual void OnInitialize() {
    }

    /// <summary>
    /// This method is executed when this system is enabling.
    /// </summary>
    protected virtual void OnStart() {
    }

    /// <summary>
    /// This method is executed on begin of every cycle of this system.
    /// </summary>
    protected virtual void OnUpdate() {
    }

    /// <summary>
    /// This method is executed on end of every cycle of this system.
    /// </summary>
    protected virtual void OnLateUpdate() {
    }

    /// <summary>
    /// This method is executed when this system is disabling.
    /// </summary>
    protected virtual void OnStop() {
    }

    /// <summary>
    /// This method is executed when this system is destroying.
    /// </summary>
    protected virtual void OnTerminate() {
    }

}
