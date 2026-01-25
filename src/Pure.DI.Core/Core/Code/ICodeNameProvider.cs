namespace Pure.DI.Core.Code;

interface ICodeNameProvider
{
    string GetConstructorName(string className);

    string GetUniqueTypeParameterName(string className);
}