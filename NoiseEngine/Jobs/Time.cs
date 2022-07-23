using System;

namespace NoiseEngine.Jobs;

internal static class Time {

    public static double UtcSeconds {
        get {
            return UtcUnixTime.TotalSeconds;
        }
    }

    public static double UtcMilliseconds {
        get {
            return UtcUnixTime.TotalMilliseconds;
        }
    }

    public static TimeSpan UtcUnixTime {
        get {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
        }
    }

}
