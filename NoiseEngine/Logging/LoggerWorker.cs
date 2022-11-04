using System;
using System.Collections.Concurrent;
using System.Threading;
using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Threading;

namespace NoiseEngine.Logging;

internal class LoggerWorker : IDisposable {

    private readonly ConcurrentQueue<LogData> queue = new ConcurrentQueue<LogData>();
    private readonly AutoResetEvent queueResetEvent = new AutoResetEvent(false);
    private readonly AutoResetEvent flushResetEvent = new AutoResetEvent(false);

    private AtomicBool isDisposed;

    public ConcurrentList<ILogSink> Sinks { get; }

    public LoggerWorker() {
        Sinks = new ConcurrentList<ILogSink>();

        new Thread(Worker) {
            IsBackground = true,
            Name = $"{nameof(Logger)} worker"
        }.Start();
    }

    public void EnqueueLog(LogData data) {
        queue.Enqueue(data);
        queueResetEvent.Set();
    }

    public void Flush() {
        // This should probably flush the sinks, but flushing is not implemented for ILogSink.
        flushResetEvent.Reset();
        queueResetEvent.Set();
        flushResetEvent.WaitOne();
    }

    public void Dispose() {
        if (!isDisposed.Exchange(true)) {
            ReleaseResources();
        }
    }

    private void Worker() {
        lock (queue) {
            while (!isDisposed) {
                queueResetEvent.WaitOne();
                DequeueLogs();
                flushResetEvent.Set();
            }
        }

        lock (queueResetEvent) {
            queueResetEvent.Dispose();
            flushResetEvent.Dispose();
        }
    }

    private void DequeueLogs() {
        while (queue.TryDequeue(out LogData data)) {
            foreach (ILogSink sink in Sinks) {
                sink.Log(data);
            }
        }
    }

    private void DisposeSinks() {
        foreach (ILogSink sink in Sinks) {
            sink.Dispose();
        }
    }

    private void ReleaseResources() {
        lock (queueResetEvent) {
            if (!queueResetEvent.SafeWaitHandle.IsClosed) {
                queueResetEvent.Set();
            }
        }

        lock (queue) {
            DequeueLogs();
            DisposeSinks();
        }
    }

}
