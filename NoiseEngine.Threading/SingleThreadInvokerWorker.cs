using System;
using System.Collections.Concurrent;
using System.Threading;
using NoiseEngine.Logging;

namespace NoiseEngine.Threading {
    internal class SingleThreadInvokerWorker : IDisposable {

        private readonly ConcurrentQueue<(Action, AutoResetEvent?)> toExecute =
            new ConcurrentQueue<(Action, AutoResetEvent?)>();

        private readonly AutoResetEvent actionAdded = new AutoResetEvent(false);

        private readonly Logger? logger;

        private bool isDisposed;

        public SingleThreadInvokerWorker(string threadName, Logger? logger) {
            this.logger = logger;

            Thread thread = new Thread(Worker) {
                Name = threadName
            };

            thread.Start();
        }

        /// <summary>
        /// Enqueues <paramref name="action"/> for execution on the worker thread.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void Execute(Action action) {
            toExecute.Enqueue((action, null));
            actionAdded.Set();
        }

        /// <summary>
        /// Enqueues <paramref name="action"/> for execution and waits for its completion.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void ExecuteAndWait(Action action) {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            toExecute.Enqueue((action, autoResetEvent));
            actionAdded.Set();
            autoResetEvent.WaitOne();
            autoResetEvent.Dispose();
        }

        public void Dispose() {
            isDisposed = true;
            actionAdded.Set();
        }

        private void Worker() {
            while (!isDisposed) {
                actionAdded.WaitOne();

                AutoResetEvent? actionAutoResetEvent = null;

                try {
                    while (toExecute.TryDequeue(out (Action action, AutoResetEvent? autoResetEvent) result)) {
                        actionAutoResetEvent = result.autoResetEvent;
                        result.action();
                        result.autoResetEvent?.Set();
                    }
                } catch (Exception e) {
                    logger?.CriticalError(e);
                } finally {
                    actionAutoResetEvent?.Set();
                }
            }

            actionAdded.Dispose();
        }

    }
}
