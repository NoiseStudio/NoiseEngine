using System;

namespace NoiseEngine.Nesl;

internal static class TypeAttributesExtensions {

    public static System.Reflection.TypeAttributes GetCilAttributes(this TypeAttributes attributes) {
        System.Reflection.TypeAttributes result = 0;

        if (attributes is TypeAttributes.Public)
            result |= System.Reflection.TypeAttributes.Public;

        if (attributes is TypeAttributes.Abstract)
            result |= System.Reflection.TypeAttributes.Abstract;

        if (attributes is TypeAttributes.Class)
            result |= System.Reflection.TypeAttributes.Class;
        else if (attributes is TypeAttributes.Struct)
            throw new NotImplementedException();
        else if (attributes is TypeAttributes.Interface)
            result |= System.Reflection.TypeAttributes.Interface;

        return result;
    }

}
