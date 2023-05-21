using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.PushConstants;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal class VertexUtils : IntrinsicsContainer {

    public VertexUtils(
        SpirVGenerator generator, SpirVFunction function, IReadOnlyList<SpirVVariable> parameters
    ) : base(generator, function, parameters) {
    }

    public override void Process() {
        switch (NeslMethod.Name) {
            case NeslOperators.PropertyGet + nameof(Default.VertexUtils.Index):
                EmitBuiltInVariable(BuiltInTypes.Int32, 42u);
                break;
            case nameof(Default.VertexUtils.ObjectToClipPos):
                ObjectToClipPos();
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

    private void ObjectToClipPos() {
        NeslType matrix4x4 = Matrices.GetMatrix4x4(BuiltInTypes.Float32);
        SpirVVariable pushConstant = Compiler.PushConstantsHelper.GetPushConstant(matrix4x4);

        Compiler.ResultBuilder.PushConstantDescriptors.Add(
            new PushConstantDescriptor(0, (int)(matrix4x4.GetSize() / 8), RenderingFeatures.ObjectToClipPos
        ));

        SpirVId positionLoad = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpLoad, Compiler.GetSpirVType(Vectors.GetVector3(BuiltInTypes.Float32)).Id,
            positionLoad, Parameters[0].GetAccess(Generator)
        );

        SpirVType floatType = Compiler.GetSpirVType(BuiltInTypes.Float32);

        SpirVId x = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpCompositeExtract, floatType.Id, x, positionLoad, 0u.ToSpirVLiteral());
        SpirVId y = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpCompositeExtract, floatType.Id, y, positionLoad, 1u.ToSpirVLiteral());
        SpirVId z = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpCompositeExtract, floatType.Id, z, positionLoad, 2u.ToSpirVLiteral());

        SpirVType vector4 = Compiler.GetSpirVType(Vectors.GetVector4(BuiltInTypes.Float32));

        SpirVId position4 = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpCompositeConstruct, vector4.Id, position4,
            stackalloc SpirVId[] { x, y, z, Compiler.GetConst(1f) }
        );

        SpirVId pushConstantLoad = Compiler.GetNextId();
        Generator.Emit(
            SpirVOpCode.OpLoad, Compiler.GetSpirVType(matrix4x4).Id, pushConstantLoad, pushConstant.GetAccess(Generator)
        );

        SpirVId result = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpMatrixTimesVector, vector4.Id, result, pushConstantLoad, position4);

        Generator.Emit(SpirVOpCode.OpReturnValue, result);
    }

}
