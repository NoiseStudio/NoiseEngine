using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal class ComputeUtils : IntrinsicsContainer {

    public ComputeUtils(
        SpirVGenerator generator, SpirVFunction function, IReadOnlyList<SpirVVariable> parameters
    ) : base(generator, function, parameters) {
    }

    public override void Process() {
        switch (NeslMethod.Name) {
            /*case NeslOperators.PropertyGet + nameof(Default.Compute.WorkgroupCount3):
                EmitBuiltInVariable(Vectors.GetVector3(BuiltInTypes.UInt32), 24u);
                break;
            case NeslOperators.PropertyGet + nameof(Default.Compute.WorkgroupSize3):
                EmitBuiltInVariable(Vectors.GetVector3(BuiltInTypes.UInt32), 25u);
                break;*/
            case NeslOperators.PropertyGet + nameof(Default.ComputeUtils.GlobalInvocation3):
                EmitBuiltInVariable(Vectors.GetVector3(BuiltInTypes.UInt32), 28u);
                break;
            default:
                throw NewUnableFindDefinitionException();
        }
    }

    private void EmitBuiltInVariable(NeslType neslType, uint index) {
        SpirVVariable variable = new SpirVVariable(Compiler, neslType, StorageClass.Input, Compiler.TypesAndVariables);
        Compiler.AddVariable(variable);
        Function.UsedIOVariables.Add(variable);

        lock (Compiler.Annotations) {
            Compiler.Annotations.Emit(
                SpirVOpCode.OpDecorate, variable.Id, (uint)Decoration.BuiltIn, index.ToSpirVLiteral()
            );
        }

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, Compiler.GetSpirVType(variable.NeslType).Id, id, variable.Id);
        Generator.Emit(SpirVOpCode.OpReturnValue, id);
    }

}
