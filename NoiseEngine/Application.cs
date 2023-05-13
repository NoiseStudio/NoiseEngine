using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Interop.Logging;
using NoiseEngine.Jobs;
using NoiseEngine.Logging;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NoiseEngine;

public static class Application {

    private static readonly object exitLocker = new object();
    private static readonly object graphicsInstanceLocker = new object();
    private static readonly ConcurrentList<ApplicationScene> loadedScenes = new ConcurrentList<ApplicationScene>();
    private static readonly ManualResetEvent endResetEvent = new ManualResetEvent(false);

    private static AtomicBool isInitialized;
    private static bool isExited;
    private static ApplicationSettings? settings;
    private static GraphicsInstance? graphicsInstance;

    public static Version? EngineVersion => typeof(Application).Assembly.GetName().Version;

    public static string Name => Settings.Name!;
    public static Version Version => Settings.Version!;
    public static EntitySchedule EntitySchedule => Settings.EntitySchedule!;
    public static Jobs2.EntitySchedule EntitySchedule2 { get; private set; } = new Jobs2.EntitySchedule();
    public static Jobs2.JobsInvoker JobsInvoker { get; private set; } = new Jobs2.JobsInvoker();

    public static IEnumerable<ApplicationScene> LoadedScenes => loadedScenes;
    public static IEnumerable<Window> Windows => WindowEventHandler.Windows;

    public static GraphicsInstance GraphicsInstance => GetGraphicsInstance();
    public static bool IsDebugMode => Settings.DebugMode!.Value;

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
            if (!Log.Logger.Sinks.Any(x => typeof(FileLogSink) == x.GetType())) {
                Log.Logger.AddSink(FileLogSink.CreateFromDirectory(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")
                ));
            }
        }

        InteropLogging.Initialize(Log.Logger);

        // Set default values.
        if (settings.Name is null || settings.Version is null || settings.DebugMode is null) {
            Assembly? entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is null) {
                settings = settings with {
                    Name = settings.Name ?? "Unknown",
                    Version = settings.Version ?? new Version(),
                    DebugMode = settings.DebugMode ?? false
                };
            } else {
                AssemblyName name = entryAssembly.GetName();

                bool debugMode;
                if (settings.DebugMode is null) {
                    DebuggableAttribute? attribute = entryAssembly.GetCustomAttribute<DebuggableAttribute>();
                    if (attribute is not null)
                        debugMode = attribute.IsJITTrackingEnabled;
                    else
                        debugMode = false;
                } else {
                    debugMode = settings.DebugMode.Value;
                }

                settings = settings with {
                    Name = settings.Name ?? name.Name ?? entryAssembly.Location,
                    Version = settings.Version ?? name.Version ?? new Version(),
                    DebugMode = debugMode
                };
            }
        }

        Application.settings = settings with {
            EntitySchedule = settings.EntitySchedule ?? new EntitySchedule()
        };

        EntitySchedule2 = new Jobs2.EntitySchedule();
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

            // Tries to collect graphics resources. This is not required, but it relieves the operating system and
            // allows to better control the engine.
            for (int i = 0; i < 16; i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Log.Info($"{nameof(Application)} exited with code {exitCode}.");
            Log.Logger.Flush();

            endResetEvent.Set();

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainOnExit;
            if (Settings.ProcessExitOnApplicationExit)
                Environment.Exit(exitCode);
        }
    }

    /// <summary>
    /// Blocks this thread until <see cref="Exit(int)"/> will be invoked.
    /// </summary>
    public static void WaitToEnd() {
        endResetEvent.WaitOne();
    }

    internal static void AddSceneToLoaded(ApplicationScene scene) {
        loadedScenes.Add(scene);
    }

    internal static void RemoveSceneFromLoaded(ApplicationScene scene) {
        loadedScenes.Remove(scene);
    }

    internal static void RaiseWindowClosed() {
        if (Settings.AutoExitWhenAllWindowsAreClosed && Windows.All(x => x.IsDisposed))
            Exit();
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
            graphicsInstance = GraphicsInstance.Create(
                Settings.DisablePresentation, Settings.DebugMode!.Value, Settings.EnableValidationLayers
            );
        }

        return graphicsInstance;
    }

    private static void AssertNotExited() {
        if (isExited)
            throw new InvalidOperationException($"{nameof(Application)} is exited.");
    }

}
