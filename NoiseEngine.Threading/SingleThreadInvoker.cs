using System;
using NoiseEngine.Logging;

namespace NoiseEngine.Threading {
    public class SingleThreadInvoker {

        private readonly SingleThreadInvokerWorker worker;
        private AtomicBool isDestroyed;

        /// <summary>
        /// Creates a <see cref="SingleThreadInvoker"/> instance.
        /// </summary>
        /// <param name="threadName">Name of the thread to spawn.</param>
        /// <param name="logger">Logger to use when exception is thrown.</param>
        public SingleThreadInvoker(string threadName = $"{nameof(SingleThreadInvoker)} worker", Logger? logger = null) {
            worker = new SingleThreadInvokerWorker(threadName, logger);
        }

        ~SingleThreadInvoker() {
            worker.Dispose();
        }

        /// <summary>
        /// Enqueues <paramref name="action"/> for execution on the worker thread.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void Execute(Action action) {
            worker.Execute(action);
        }

        /// <summary>
        /// Enqueues <paramref name="action"/> for execution and waits for its completion.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        public void ExecuteAndWait(Action action) {
            worker.ExecuteAndWait(action);
        }

        /// <summary>
        /// Signals worker thread to stop after executing remaining actions.
        /// This method does not wait for thread to finish.
        /// </summary>
        public void Destroy() {
            if (isDestroyed.Exchange(true))
                return;

            worker.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
