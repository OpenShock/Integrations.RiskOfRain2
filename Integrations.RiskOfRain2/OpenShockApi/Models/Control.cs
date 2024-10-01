using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;

namespace OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class Control
{
    public Guid Id { get; set; }
    public ControlType Type { get; set; }
    public byte Intensity { get; set; }
    public ushort Duration { get; set; }
}