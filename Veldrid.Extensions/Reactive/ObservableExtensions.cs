using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Veldrid.Extensions.Reactive
{
    public static class ObservableExtensions
    {
        #region Polling pattern
        // Implementation details are irrelevant to users
        internal class PollingWrapper<TState, T> : IObservable<T>
        {
            private IObserver<T> _observer;
            private readonly Task _task;
            private T _lastValue;
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly PauseTokenSource _pts = new PauseTokenSource();
            private bool _validLastValue;

            public PollingWrapper(TState state, Func<TState, T> refresh, ulong timeToSleepMs = 0)
            {
                // Don't start unless we have an observer
                _pts.IsPaused = _observer == null;

                // Start our polling task
                _task = Task.Factory.StartNew(s =>
                {
                    // TODO: Make this a better game loop with desiredFPS and delta time instead
                    var timeToSleep = TimeSpan.FromMilliseconds(timeToSleepMs);

                    // Poll until we've been asked to cancel
                    while (!_cts.IsCancellationRequested)
                        try
                        {
                            // Don't proceed if paused
                            if (_pts.IsPaused)
                                _pts.Token.WaitWhilePaused();

                            // Refresh our value using the Func provided
                            var value = refresh(state);
                            // Let the observer know we have a new value
                            _observer?.OnNext(value);

                            // Remember this for subscribers that arrive too late for this notification
                            _lastValue = value;
                            _validLastValue = true;

                            // Sleep for the time indicated so we aren't running too fast
                            Thread.Sleep(timeToSleep);
                        }
                        catch (Exception ex)
                        {
                            // We encountered an exception, let the observer know
                            _observer?.OnError(ex);
                            continue; // Keep going
                        }

                    // All done
                    _observer.OnCompleted();
                }, state, _cts.Token);
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                _observer = observer;
                _pts.IsPaused = false;

                // So new subscribers still receive a value
                if (_validLastValue)
                    _observer?.OnNext(_lastValue);

                return Disposable.Create(() => { _observer = null; _pts.IsPaused = true; });
            }

            public void Dispose()
            {
                // Cancel game loop
                if (!_cts.IsCancellationRequested)
                    _cts.Cancel();

                // Make sure our task is complete and done
                _task.Wait();
            }

            public PauseTokenSource PauseTokenSource => _pts;
            public CancellationTokenSource CancellationTokenSource => _cts;
        }

        public static IObservable<T> FromPollingPattern<TState, T>(this TState state, Func<TState, T> refresh, out PauseTokenSource pts, out CancellationTokenSource cts, ulong timeToSleepMs = 0)
        {
            var result = new PollingWrapper<TState, T>(state, refresh, timeToSleepMs);
            cts = result.CancellationTokenSource;
            pts = result.PauseTokenSource;

            return result;
        }

        #endregion`

        #region File watcher

        public static IObservable<FileSystemEventArgs> ObserveChanges<T>(this string filename)
        {
            var fsw = new FileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename)) 
            { 
                EnableRaisingEvents = true 
            };
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (sender, e) => handler(e),
                fsHandler => fsw.Changed += fsHandler,
                fsHandler => fsw.Changed -= fsHandler);
        }

        #endregion
    }
}