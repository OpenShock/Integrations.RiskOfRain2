using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
    private OpenShockApi.OpenShockApi? _openShockApiClient = null;
    private IList<Guid> _shockersList = null!;
    
    private void SetupApiClient()
    {
        if (!Uri.TryCreate(_openShockServer.Value, UriKind.Absolute, out var serverUri))
        {
            Logger.LogWarning("Unable to parse OpenShock server URL. E.g. https://api.openshock.app");
            return;
        }

        if (string.IsNullOrWhiteSpace(_openShockApiToken.Value))
        {
            Logger.LogWarning("API Token is not configured");
            return;
        }

        _openShockApiClient = new OpenShockApi.OpenShockApi(serverUri, _openShockApiToken.Value);
    }
    
    private void SetupShockers()
    {
        var newList = new List<Guid>();
        foreach (var shocker in _shockers.Value.Split(','))
        {
            if (Guid.TryParse(shocker, out var shockerGuid))
            {
                Logger.LogInfo("Found shocker ID " + shockerGuid);
                newList.Add(shockerGuid);
            }
            else Logger.LogError($"Failed to parse shocker ID {shocker}");
        }

        _shockersList = newList;
    }

    private async Task ControlShockers(ControlType controlType, byte intensity, ushort duration)
    {
        if (_openShockApiClient == null)
        {
            Logger.LogWarning("OpenShock server or token not configured");
            return;
        }

        Logger.LogDebug("Sending control request");
        
        var controls = _shockersList.Select(shocker =>
            new Control
            {
                Id = shocker,
                Duration = duration,
                Intensity = intensity,
                Type = controlType
            });

        await _openShockApiClient.Control(controls);

        Logger.LogDebug("Command sent");
    }
    
    private void ControlShockersFnf(ControlType controlType, byte intensity, ushort duration) =>
        LucTask.Run(() => ControlShockers(controlType, intensity, duration));

}