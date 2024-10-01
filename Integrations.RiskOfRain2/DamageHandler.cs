using OpenShock.Integrations.RiskOfRain2.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;
using RoR2;
using MathUtils = OpenShock.Integrations.RiskOfRain2.Utils.MathUtils;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
    private void ClientOnDamage(DamageDealtMessage damageMessage)
    {
        if (_localUserCharacterBody == null || damageMessage.victim == null) return;
        if (damageMessage.victim != _localUserCharacterBody.gameObject) return;
        
        var intensityByte = CalculateIntensity(damageMessage);

        Logger.LogDebug($"Player received damage: {damageMessage.damage}");

        ControlShockersFnf(_settingOnDamageMode.Value, intensityByte, (ushort)_settingOnDamageDuration.Value);
    }

    private byte CalculateIntensity(DamageDealtMessage damageMessage)
    {
#pragma warning disable CS8602 // This is fine, we checked this before
        var playerMaxHp = _localUserCharacterBody.healthComponent.fullCombinedHealth;
#pragma warning restore CS8602
        
        var playerCurrentHp = _localUserCharacterBody.healthComponent.health;
        var onDamageBehaviour = _settingOnDamageBehaviour.Value;
        float intensity;
        
        if (onDamageBehaviour is DamageBehaviour.LowHp or DamageBehaviour.DamagePercent)
        {
            var scaled = onDamageBehaviour switch
            {
                DamageBehaviour.LowHp => playerCurrentHp / playerMaxHp * 100,
                DamageBehaviour.DamagePercent => damageMessage.damage / playerMaxHp * 100,
                _ => throw new NotImplementedException("This is unreachable.")
            };
            intensity = MathUtils.Lerp(0, _settingOnDamageIntensityLimit.Value, Math.Clamp(scaled, 0, 100));
        }
        else // Only other option is DamageAbsolute
        {
            intensity = Math.Clamp(damageMessage.damage, 0, _settingOnDamageIntensityLimit.Value);
        }
        
        var intensityByte = Convert.ToByte(intensity);
        return intensityByte;
    }
}