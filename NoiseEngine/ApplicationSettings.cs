using NoiseEngine.Jobs;
using System;

namespace NoiseEngine;

public readonly record struct ApplicationSettings(
    string? Name = null,
    Version? Version = null,
    EntitySchedule? EntitySchedule = null,
    bool? DebugMode = null,
    bool AddDefaultLoggerSinks = true,
    bool AutoExitWhenAllWindowsAreClosed = true,
    bool ProcessExitOnApplicationExit = true,
    bool DisablePresentation = false,
    bool EnableValidationLayers = false
) {

    public ApplicationSettings() : this(Name: null) {
    }

}
