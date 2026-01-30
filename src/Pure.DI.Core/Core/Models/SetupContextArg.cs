namespace Pure.DI.Core.Models;

readonly record struct SetupContextArg(
    ITypeSymbol Type,
    string Name);
