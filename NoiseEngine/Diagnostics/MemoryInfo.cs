﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using NoiseEngine.Interop.InteropMarshalling;

namespace NoiseEngine.Diagnostics;

public static class MemoryInfo {

    public static nuint TotalPhysicalMemory { get; }
    public static nuint AvailablePhysicalMemory => GetAvailablePhysicalMemory();

    public static nuint UsedPhysicalMemory => TotalPhysicalMemory - AvailablePhysicalMemory;
    public static float PercentUsedPhysicalMemory => (float)(UsedPhysicalMemory / (decimal)TotalPhysicalMemory);
    public static float PercentAvailablePhysicalMemory =>
        (float)(AvailablePhysicalMemory / (decimal)TotalPhysicalMemory);

    static MemoryInfo() {
        if (OperatingSystem.IsWindows()) {
            PerformanceInformation information = GetWindowsPerformanceInfo();
            TotalPhysicalMemory = information.PhysicalTotal * information.PageSize;
        } else if (OperatingSystem.IsLinux()) {
            TotalPhysicalMemory = GetLinuxMemoryInfo("MemTotal");
        } else {
            throw new NotImplementedException();
        }
    }

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern InteropBool GetPerformanceInfo(
        [Out] out PerformanceInformation information, [In] int size);

    private static PerformanceInformation GetWindowsPerformanceInfo() {
        if (!GetPerformanceInfo(out PerformanceInformation information, Marshal.SizeOf<PerformanceInformation>()))
            throw new NotImplementedException();
        return information;
    }

    private static nuint GetLinuxMemoryInfo(string key) {
        string[] resultSplit = File.ReadAllLines("/proc/meminfo").First(x => x.StartsWith(key))
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (resultSplit[2].Length > 0 && resultSplit[2] != "kB")
            throw new NotImplementedException();

        return nuint.Parse(resultSplit[1]) * 1024;
    }

    private static nuint GetAvailablePhysicalMemory() {
        if (OperatingSystem.IsWindows()) {
            PerformanceInformation information = GetWindowsPerformanceInfo();
            return information.PhysicalAvailable * information.PageSize;
        } else if (OperatingSystem.IsLinux()) {
            return GetLinuxMemoryInfo("MemFree");
        }

        throw new NotImplementedException();
    }

}
