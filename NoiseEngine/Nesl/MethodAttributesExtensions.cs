namespace NoiseEngine.Nesl;

internal static class MethodAttributesExtensions {

    public static System.Reflection.MethodAttributes GetCilAttributes(this MethodAttributes attributes) {
        System.Reflection.MethodAttributes result = 0;

        if (attributes.HasFlag(MethodAttributes.Private))
            result |= System.Reflection.MethodAttributes.Private;
        else if (attributes.HasFlag(MethodAttributes.Public))
            result |= System.Reflection.MethodAttributes.Public;

        if (attributes.HasFlag(MethodAttributes.Static))
            result |= System.Reflection.MethodAttributes.Static;
        if (attributes.HasFlag(MethodAttributes.Abstract))
            result |= System.Reflection.MethodAttributes.Abstract;
        if (attributes.HasFlag(MethodAttributes.Virtual))
            result |= System.Reflection.MethodAttributes.Virtual;

        return result;
    }

}
