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
                if (!enumerator.MoveNext())
                {
                    variants = null;
                    return false;
                }

                if (enumerator.Current is {} current)
                {
                    initial.Add(current);
                }
            }

            variants = initial;
            return true;
        }

        for (var index = 0; index < enumerators.Count; index++)
        {
            var enumerator = enumerators[index];
            if (!enumerator.MoveNext())
            {
                continue;
            }

            for (var resetIndex = 0; resetIndex < index; resetIndex++)
            {
                var resetEnumerator = enumerators[resetIndex];
                resetEnumerator.Reset();
                if (resetEnumerator.MoveNext())
                {
                    continue;
                }

                variants = null;
                return false;
            }

            variants = enumerators.Select(i => i.Current!).ToList();
            return true;
        }

        variants = null;
        return false;
    }
}
