using System.Collections;

namespace Pure.DI.Core.Models;

record InitializersWalkerContext(
    Func<VarInjection, IEnumerator> BuildVarInjection,
    string VariableName,
    IEnumerator<VarInjection> VarInjections);