using System;

namespace NoiseEngine.Nesl;

internal static class TypeAttributesExtensions {

    public static System.Reflection.TypeAttributes GetCilAttributes(this TypeAttributes attributes) {
        System.Reflection.TypeAttributes result = 0;

        if (attributes.HasFlag(TypeAttributes.Public))
            result |= System.Reflection.TypeAttributes.Public;

        if (attributes.HasFlag(TypeAttributes.Abstract))
            result |= System.Reflection.TypeAttributes.Abstract;

        if (attributes.HasFlag(TypeAttributes.Class))
            result |= System.Reflection.TypeAttributes.Class;
        else if (attributes.HasFlag(TypeAttributes.Struct))
            throw new NotImplementedException();
        else if (attributes.HasFlag(TypeAttributes.Interface))
            result |= System.Reflection.TypeAttributes.Interface;

        return result;
    }

}
