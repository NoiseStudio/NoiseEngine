namespace NoiseEngine.Interop;

internal enum ResultErrorKind : uint {
    Universal = 0,
    LibraryLoad = 1,
    NullReference = 2,
    InvalidOperation = 3,
    Overflow = 4,
    Argument = 5,

    GraphicsUniversal = 1000,
    GraphicsInstanceCreate = 1001,
    GraphicsOutOfHostMemory = 1002,
    GraphicsOutOfDeviceMemory = 1003,
    GraphicsDeviceLost = 1004
}
