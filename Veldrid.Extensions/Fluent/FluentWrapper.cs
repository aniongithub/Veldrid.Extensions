namespace Veldrid.Extensions.Fluent
{
    public sealed class FluentWrapper<T>
    {
        internal readonly T Value;

        internal FluentWrapper(T value)
        {
            Value = value;
        }
    }
}