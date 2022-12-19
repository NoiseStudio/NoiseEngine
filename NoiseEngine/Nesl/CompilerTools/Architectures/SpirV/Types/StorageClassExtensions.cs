namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

internal static class StorageClassExtensions {

    /// <summary>
    /// https://community.khronos.org/t/spirv-val-prohibits-passing-storageclass-uniform-pointer-to-function/106436/4
    /// </summary>
    public static bool IsDynamicInLogicalAddressingModel(this StorageClass storageClass) {
        return storageClass switch {
            StorageClass.UniformConstant => true,
            StorageClass.Function => true,
            StorageClass.Private => true,
            StorageClass.Workgroup => true,
            StorageClass.AtomicCounter => true,
            _ => false
        };
    }

}
