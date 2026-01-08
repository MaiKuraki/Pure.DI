namespace Pure.DI.Core.Models;

record CodeBuilderContext(
    CodeContext Context,
    ICollection<VarInjection> VarInjections);
