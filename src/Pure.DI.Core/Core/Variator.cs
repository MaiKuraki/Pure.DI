// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable IdentifierTypo

namespace Pure.DI.Core;

sealed class Variator<T> : IVariator<T>
    where T : class
{
    public bool TryGetNextVariants(
        IEnumerable<IEnumerator<T>> variations,
        [NotNullWhen(true)] out IReadOnlyCollection<T>? variants)
    {
        var enumerators = variations.ToList();
        if (enumerators.Count == 0)
        {
            variants = null;
            return false;
        }

        var allNotStarted = enumerators.All(i => i is SafeEnumerator<T> { IsStarted: false });
        var needsInitialization = allNotStarted || enumerators.Any(i => i is not SafeEnumerator<T> && i.Current is null);
        if (needsInitialization)
        {
            var initial = new List<T>(enumerators.Count);
            foreach (var enumerator in enumerators)
            {
                if (!enumerator.MoveNext() || enumerator.Current is null)
                {
                    variants = null;
                    return false;
                }

                initial.Add(enumerator.Current);
            }

            variants = initial;
            return true;
        }

        for (var index = 0; index < enumerators.Count; index++)
        {
            var enumerator = enumerators[index];
            if (!enumerator.MoveNext() || enumerator.Current is null)
            {
                continue;
            }

            for (var resetIndex = 0; resetIndex < index; resetIndex++)
            {
                var resetEnumerator = enumerators[resetIndex];
                resetEnumerator.Reset();
                if (resetEnumerator.MoveNext() && resetEnumerator.Current is not null)
                {
                    continue;
                }

                variants = null;
                return false;
            }

            var current = enumerators.Select(i => i.Current).ToList();
            if (current.Any(i => i is null))
            {
                variants = null;
                return false;
            }

            variants = current.Select(i => i!).ToList();
            return true;
        }

        variants = null;
        return false;
    }
}
