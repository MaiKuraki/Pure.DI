using Pure.DI;
using UnityEngine;
using static Pure.DI.Lifetime;

public partial class ProjectScope : BaseComposition
{
    void Setup() => DI.Setup()
        .Bind<IClockFactory>().To<ProjectFactory>() // You can register all factories implementations here
        .Root<ProjectManager>(nameof(ProjectManager))
        .Builders<MonoBehaviour>();

    void Start()
    {
        ProjectManager.Initialize();
    }

    void OnDestroy()
    {
        Dispose();
    }
}
