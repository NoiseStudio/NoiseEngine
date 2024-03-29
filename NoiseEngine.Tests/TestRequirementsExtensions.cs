﻿using System;

namespace NoiseEngine.Tests;

internal static class TestRequirementsExtensions {

    private const string Reason = "The test was skipped due to the lack of {0} support on this computer.";
    private const string EnvironmentVariable = "NOISEENGINE_NO_SUPPORTS";

    public static string ToSkipReason(this TestRequirements requirements) {
        string? args = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (args == null)
            return string.Empty;

        args = args.ToLower();

        if (requirements.HasFlag(TestRequirements.Graphics) && args.Contains("graphics;"))
            return string.Format(Reason, "graphics");
        if (requirements.HasFlag(TestRequirements.Gui) && args.Contains("gui;"))
            return string.Format(Reason, "GUI");
        if (requirements.HasFlag(TestRequirements.Vulkan) && (args.Contains("vulkan;") || args.Contains("graphics;")))
            return string.Format(Reason, "Vulkan");

        return string.Empty;
    }

}
