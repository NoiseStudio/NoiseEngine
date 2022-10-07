namespace NoiseEngine.Interop;

internal enum ResultErrorKind : uint {
    Universal = 0,
    LibraryLoad = 1,

    GraphicsInstanceCreate = 1000,
    GraphicsOutOfHostMemory = 1001,
    GraphicsOutOfDeviceMemory = 1002
}
