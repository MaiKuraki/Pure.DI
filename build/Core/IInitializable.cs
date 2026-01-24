namespace Build.Core;

interface IInitializable
{
    // ReSharper disable once UnusedParameter.Global
    Task InitializeAsync(CancellationToken cancellationToken);
}