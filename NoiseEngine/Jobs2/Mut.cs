using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2;

public readonly ref struct Mut<T> where T : IComponent {

    private readonly unsafe void* pointer;

    public ref T Value {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            unsafe {
                return ref Unsafe.AsRef<T>(pointer);
            }
        }
    }

    internal unsafe Mut(void* pointer) {
        this.pointer = pointer;
    }

}
