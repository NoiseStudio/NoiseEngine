using NoiseEngine.Jobs;

namespace NoiseEngine;

public readonly record struct ApplicationSettings(
    string? Name = null,
    EntitySchedule? EntitySchedule = null,
    bool AddDefaultLoggerSinks = true,
    bool AutoExitWhenAllWindowsAreClosed = true,
    bool ProcessExitOnApplicationExit = true
) {

    public ApplicationSettings() : this(Name: null) {
    }

}
