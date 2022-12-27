using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Interop.Logging;
using NoiseEngine.Jobs;
using NoiseEngine.Logging;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NoiseEngine;

public static class Application {

    private static readonly object exitLocker = new object();
    private static readonly object graphicsInstanceLocker = new object();
    private static readonly ConcurrentList<ApplicationScene> loadedScenes = new ConcurrentList<ApplicationScene>();

    private static AtomicBool isInitialized;
    private static bool isExited;
    private static ApplicationSettings? settings;
    private static GraphicsInstance? graphicsInstance;

    public static Version? EngineVersion => typeof(Application).Assembly.GetName().Version;

    public static string Name => Settings.Name!;
    public static Version Version => Settings.Version!;
    public static EntitySchedule EntitySchedule => Settings.EntitySchedule!;

    public static IEnumerable<ApplicationScene> LoadedScenes => loadedScenes;
    //public static IEnumerable<Window> Windows => LoadedScenes.SelectMany(x => x.Cameras).Select(x => x.RenderTarget);

    public static GraphicsInstance GraphicsInstance => GetGraphicsInstance();

    internal static ApplicationSettings Settings => settings ?? throw new InvalidOperationException(
        $"{nameof(Application)} has not been initialized with a call to {nameof(Initialize)}.");

    /// <summary>
    /// Exit handler.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    public delegate void ApplicationExitHandler(int exitCode);

    /// <summary>
    /// This event is executed when <see cref="Exit(int)"/> is called.
    /// </summary>
    public static event ApplicationExitHandler? ApplicationExit;

    /// <summary>
    /// Initializes <see cref="Application"/>.
    /// </summary>
    /// <param name="settings">Application settings.</param>
    /// <exception cref="InvalidOperationException"><see cref="Application"/> has been already initialized.</exception>
    public static void Initialize(ApplicationSettings settings) {
        if (isInitialized.Exchange(true))
            throw new InvalidOperationException($"{nameof(Application)} has been already initialized.");

        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnExit;

        if (settings.AddDefaultLoggerSinks) {
            if (!Log.Logger.Sinks.Any(x => typeof(ConsoleLogSink) == x.GetType()))
                Log.Logger.AddSink(new ConsoleLogSink(new ConsoleLogSinkSettings { ThreadNameLength = 20 }));
            if (!Log.Logger.Sinks.Any(x => typeof(FileLogSink) == x.GetType()))
                Log.Logger.AddSink(FileLogSink.CreateFromDirectory("logs"));
        }

        InteropLogging.Initialize(Log.Logger);

        // Set default values.
        if (settings.Name is null || settings.Version is null) {
            Assembly? entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is null) {
                settings = settings with {
                    Name = settings.Name is null ? "Unknown" : settings.Name,
                    Version = settings.Version is null ? new Version() : settings.Version
                };
            } else {
                AssemblyName name = entryAssembly.GetName();

                settings = settings with {
                    Name = settings.Name is null ? (name.Name ?? entryAssembly.Location) : settings.Name,
                    Version = settings.Version is null ? name.Version ?? new Version() : settings.Version
                };
            }
        }

        Application.settings = settings with {
            EntitySchedule = settings.EntitySchedule ?? new EntitySchedule()
        };
    }

    /// <summary>
    /// Disposes <see cref="Application"/> resources and when ProcessExitOnApplicationExit
    /// setting is <see langword="true"/> ends process with given <paramref name="exitCode"/>.
    /// </summary>
    /// <param name="exitCode">
    /// The exit code to return to the operating system. Use 0 (zero)
    /// to indicate that the process completed successfully.
    /// </param>
    public static void Exit(int exitCode = 0) {
        lock (exitLocker) {
            if (isExited)
                return;
            isExited = true;

            ApplicationExit?.Invoke(exitCode);

            foreach (ApplicationScene scene in LoadedScenes)
                scene.Dispose();

            EntitySchedule.Dispose();

            graphicsInstance = null;
            for (int i = 0; i < 16; i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Log.Info($"{nameof(Application)} exited with code {exitCode}.");
            Log.Logger.Flush();

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainOnExit;
            if (Settings.ProcessExitOnApplicationExit)
                Environment.Exit(exitCode);
        }
    }

    internal static void AddSceneToLoaded(ApplicationScene scene) {
        loadedScenes.Add(scene);
    }

    internal static void RemoveSceneFromLoaded(ApplicationScene scene) {
        loadedScenes.Remove(scene);
    }

    private static void CurrentDomainOnExit(object? sender, EventArgs e) {
        string info = $"The process was closed without calling {nameof(Application)}.{nameof(Exit)} method.";

        Log.Fatal(info);
        Log.Logger.Flush();

        throw new ApplicationException(info);
    }

    private static GraphicsInstance GetGraphicsInstance() {
        if (graphicsInstance is not null)
            return graphicsInstance;

        lock (graphicsInstanceLocker) {
            if (graphicsInstance is not null)
                return graphicsInstance;

            AssertNotExited();
            graphicsInstance = GraphicsInstance.Create();
        }

        return graphicsInstance;
    }

    private static void AssertNotExited() {
        if (isExited)
            throw new InvalidOperationException($"{nameof(Application)} is exited.");
    }

}
