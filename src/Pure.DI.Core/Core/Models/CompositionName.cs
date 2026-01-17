namespace Pure.DI.Core.Models;

readonly record struct CompositionName(string ClassName, string Namespace, SyntaxNode? Source)
    : IComparable<CompositionName>
{
    public string FullName { get; } =
        string.IsNullOrWhiteSpace(Namespace) ? ClassName : Namespace + "." + ClassName;

    public int CompareTo(CompositionName other) =>
        Comparer.DefaultInvariant.Compare(FullName, other.FullName);

    public bool Equals(CompositionName other) => FullName == other.FullName;

    public override int GetHashCode() => FullName.GetHashCode();

    public override string ToString() => FullName;
}