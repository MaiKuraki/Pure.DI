#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
namespace Pure.DI.Core;

sealed class SafeEnumerator<T>(IEnumerator<T> source) : IEnumerator<T>
    where T : class
{
    private bool _result;
    private T? _current;

    public bool IsStarted { get; private set; }

    public T? Current
    {
        get
        {
            if (!_result)
            {
                return _current;
            }

            _current = source.Current;
            return _current;
        }
    }

    object? IEnumerator.Current => Current;

    public bool MoveNext()
    {
        IsStarted = true;
        _result = source.MoveNext();
        if (_result)
        {
            _current = source.Current;
        }

        return _result;
    }

    public void Reset()
    {
        source.Reset();
        _result = false;
        _current = null;
        IsStarted = false;
    }

    public void Dispose() => source.Dispose();
}
