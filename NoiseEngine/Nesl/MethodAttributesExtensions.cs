namespace NoiseEngine.Nesl;

internal static class MethodAttributesExtensions {

    public static System.Reflection.MethodAttributes GetCilAttributes(this MethodAttributes attributes) {
        System.Reflection.MethodAttributes result = 0;

        if (attributes is MethodAttributes.Private)
            result |= System.Reflection.MethodAttributes.Private;
        else if (attributes is MethodAttributes.Public)
            result |= System.Reflection.MethodAttributes.Public;

        if (attributes is MethodAttributes.Static)
            result |= System.Reflection.MethodAttributes.Static;
        if (attributes is MethodAttributes.Abstract)
            result |= System.Reflection.MethodAttributes.Abstract;
        if (attributes is MethodAttributes.Virtual)
            result |= System.Reflection.MethodAttributes.Virtual;

        return result;
    }

}
