using System;
using System.Collections.Generic;

namespace Veldrid.Fluent
{
    public static class DisposableExtensions
    {
        public static void DisposeAll<T>(this IEnumerable<T> disposables) where T : IDisposable
        {
            foreach (var item in disposables)
                item.Dispose();
        }
    }

    public interface IDisposable<T>: IDisposable
    {
        T Value { get; }
    }

    public sealed class DisposableWrapper<T> : IDisposable<T>
    {
        private readonly T _value;
        private Action<T> _disposeFunc;

        public DisposableWrapper(T value, Action<T> disposeFunc)
        {
            _value = value;
            _disposeFunc = disposeFunc;
        }

        public void Dispose()
        {
            if (_disposeFunc != null)
                _disposeFunc(_value);
        }

        public T Value => _value;

        public static implicit operator T(DisposableWrapper<T> operand)
        {
            return operand.Value;
        }
    }
}