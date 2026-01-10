using Pure.DI;
using UnityEngine;

public partial class ProjectScope : MonoBehaviour, IDatetimeComposition, IClockComposition
{
    [SerializeField] public DatetimeConfig datetimeConfig;
    [SerializeField] public ClockConfig clockConfig;

    public DatetimeConfig DatetimeConfig => datetimeConfig;
    public ClockConfig ClockConfig => clockConfig;

    void Setup() => DI.Setup()
        .Transient<ProjectFactory>() // You can register all factories implementations here
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
