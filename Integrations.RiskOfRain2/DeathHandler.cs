using OpenShock.Integrations.RiskOfRain2.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;
using RoR2;
using MathUtils = OpenShock.Integrations.RiskOfRain2.Utils.MathUtils;
using UnityEngine;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
    // Remember when we last died so we can ignore damage events right after death
    double _lastDeathTime = -1;
    private void OnCharacterDeath(DamageReport damageReport)
    {
        if (_localUserCharacterBody == null || damageReport.victimBody == null) return;
        if (damageReport.victimBody != _localUserCharacterBody) return;

        Logger.LogDebug("Player has died");

        if (!_settingOnDeathEnabled.Value)
        {
            Logger.LogDebug("OnDeath is disabled, skipping");
            return;
        }

        _lastDeathTime = Time.realtimeSinceStartupAsDouble;

        ControlShockersFnf(
            _settingOnDeathBehaviour.Value,
            (byte)_settingOnDeathIntensity.Value,
            (ushort)_settingOnDeathDuration.Value);
    }
}