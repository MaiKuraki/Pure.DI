namespace Pure.DI.Core.Code;

/// <summary>
/// Provides tools for working with variables.
/// </summary>
interface IVariableTools
{
    /// <summary>
    /// Returns a comparer for sorting injections.
    /// </summary>
    Comparison<VarInjection> InjectionComparer { get; }
}
