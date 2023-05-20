namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal static class NamespaceUtils {

    public static bool IsPartOf(string lhs, string rhs) {
        if (lhs == rhs)
            return true;

        string[] lp = lhs.Split('.');
        string[] rp = rhs.Split('.');
        if (lp.Length > rp.Length)
            return false;

        for (int i = 0; i < lp.Length; i++) {
            if (lp[i] != rp[i])
                return false;
        }
        return true;
    }

}
