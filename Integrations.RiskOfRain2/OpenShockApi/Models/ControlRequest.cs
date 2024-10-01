using System.Collections.Generic;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;

namespace OpenShock.Integrations.LethalCompany.OpenShockApi.Models;

public class ControlRequest
{
    public IEnumerable<Control> Shocks { get; set; } = null!;
    public string? CustomName { get; set; }
}