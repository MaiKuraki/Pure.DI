using Pure.DI;
using UnityEngine;
using Object = UnityEngine.Object;

public class ProjectFactory : IClockFactory
{
    private readonly ProjectScope _scope;

    public ProjectFactory(ProjectScope scope)
    {
        _scope = scope;
    }

    public ClockDigital Instantiate(ClockDigital prefab)
    {
        var instance = Object.Instantiate(prefab);

        _scope.BuildUp(instance);

        return instance;
    }
}