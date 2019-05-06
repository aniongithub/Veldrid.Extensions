using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Veldrid.Extensions
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
                // Make sure 
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

        #region JSON config file

        internal class JsonFileWatcher<T> : IObservable<T>, IDisposable
        {
            private readonly FileSystemWatcher _fsw = new FileSystemWatcher { EnableRaisingEvents = false };
            private IObserver<T> _observer;
            private StreamReader _reader;

            public JsonFileWatcher(string filename)
            {
                // Open in non-exclusive read mode
                _reader = new StreamReader(File.Open(filename,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite));
                _fsw.Path = Path.GetDirectoryName(filename);
                _fsw.Filter = Path.GetFileName(filename);
                _fsw.Changed += (sender, e) =>
                {
                    try
                    {
                        Thread.Sleep(250);

                        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        var readText = _reader.ReadToEnd();
                        _observer?.OnNext(JsonConvert.DeserializeObject<T>(readText));
                    }
                    catch (Exception ex)
                    {
                        _observer?.OnError(ex);
                    }
                };
            }

            public void Dispose()
            {
                _fsw.Dispose();
                _reader.Dispose();
                _observer?.OnCompleted();
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                _observer = observer;
                _fsw.EnableRaisingEvents = true;

                // Raise event with current data for new subscribers
                _reader.BaseStream.Seek(0, SeekOrigin.Begin);
                _observer?.OnNext(JsonConvert.DeserializeObject<T>(_reader.ReadToEnd()));

                return Disposable.Create(() =>
                {
                    _fsw.EnableRaisingEvents = false;
                    _observer = null;
                });
            }
        }

        public static IObservable<T> FromJSONFile<T>(this string filename)
        {
            return new JsonFileWatcher<T>(filename);
        }

        #endregion
    }
}