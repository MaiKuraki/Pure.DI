namespace Pure.DI.Core.Code;

/// <summary>
/// Represents a variable declaration in the generated code.
/// </summary>
/// <param name="NameProvider">The name provider.</param>
/// <param name="PerLifetimeId">The ID per lifetime.</param>
/// <param name="Node">The dependency node.</param>
record VarDeclaration(
    INameProvider NameProvider,
    int PerLifetimeId,
    IDependencyNode Node)
{
    private readonly Lazy<string> _name = new(() => NameProvider.GetVariableName(Node, PerLifetimeId));

    /// <summary>
    /// Gets or sets a value indicating whether the variable has been declared.
    /// </summary>
    public bool IsDeclared { get; set; } = IsDeclaredDefault(Node) ;

    /// <summary>
    /// Gets the type of the instance.
    /// </summary>
    public ITypeSymbol InstanceType => Node.Node.Type;

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public string Name => _name.Value;

    /// <summary>
    /// Resets the declaration to its default state.
    /// </summary>
    /// <returns>True if the state has changed.</returns>
    public bool ResetToDefaults()
    {
        var declaredDefault = IsDeclaredDefault(Node);
        if (declaredDefault == IsDeclared)
        {
            return false;
        }

        IsDeclared = declaredDefault;
        return true;
    }

    /// <summary>
    /// Resets only the mutable state of the declaration.
    /// </summary>
    /// <returns>True if the state has changed.</returns>
    public bool ResetStateToDefaults() => ResetToDefaults();

    public override string ToString() => $"{InstanceType} {Name}";

    private static bool IsDeclaredDefault(IDependencyNode node) =>
        node.ActualLifetime is Lifetime.Singleton or Lifetime.Scoped or Lifetime.PerResolve || node.Arg is not null;
}