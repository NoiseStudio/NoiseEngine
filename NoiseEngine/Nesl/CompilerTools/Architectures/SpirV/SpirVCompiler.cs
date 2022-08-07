using NoiseEngine.Collections;
using System;
using System.Buffers.Binary;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVCompiler {

    private readonly FastList<byte> firstWords = new FastList<byte>();

    public NeslAssembly NeslAssembly { get; }

    public SpirVCompiler(NeslAssembly neslAssembly) {
        NeslAssembly = neslAssembly;
    }

    /// <summary>
    /// https://registry.khronos.org/SPIR-V/specs/1.2/SPIRV.html#_a_id_physicallayout_a_physical_layout_of_a_spir_v_module_and_instruction
    /// </summary>
    private void FirstWords() {
        Span<byte> bytes = stackalloc byte[sizeof(int)];

        BinaryPrimitives.WriteInt32BigEndian(bytes, 0x07230203); // Magic number.

        firstWords.AddRange();
    }

}
