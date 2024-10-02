using OpenShock.Integrations.RiskOfRain2.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;
using RoR2;
using MathUtils = OpenShock.Integrations.RiskOfRain2.Utils.MathUtils;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
    private Timer _actionTimer = null!;
    private readonly object _receivedDamageLock = new();
    private float _receivedDamage = 0;

    private void ActionTimerElapsed(object state)
    {
        try
        {
            OnDamageActionExecute();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    private void OnDamageActionExecute()
    {
        float receivedDamage;
        lock (_receivedDamageLock)
        {
            receivedDamage = _receivedDamage;
            _receivedDamage = 0;
        }
        
        var intensityByte = CalculateIntensity(receivedDamage);
        
        Logger.LogDebug($"ActionTimerElapsed - Damage: {receivedDamage:0.00} Intensity: {intensityByte}");
        
        ControlShockersFnf(_settingOnDamageMode.Value, intensityByte, (ushort)_settingOnDamageDuration.Value);
    }

    private void ClientOnDamage(DamageDealtMessage damageMessage)
    {
        if (_localUserCharacterBody == null || damageMessage.victim == null) return;
        if (damageMessage.victim != _localUserCharacterBody.gameObject) return;
        
        Logger.LogDebug($"Player received damage: {damageMessage.damage}");
        
        lock (_receivedDamageLock)
        {
            _receivedDamage += damageMessage.damage;            
        }

        _actionTimer.Change(TimeSpan.FromMilliseconds(50), Timeout.InfiniteTimeSpan);
    }

    private byte CalculateIntensity(float damage)
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
                DamageBehaviour.LowHp => playerCurrentHp / playerMaxHp,
                DamageBehaviour.DamagePercent => damage / playerMaxHp,
                _ => throw new Exception("This is unreachable.")
            };
            intensity = MathUtils.Lerp(0, _settingOnDamageIntensityLimit.Value, MathUtils.Saturate(scaled));
        }
        else // Only other option is DamageAbsolute
        {
            intensity = Math.Clamp(damage, 0, _settingOnDamageIntensityLimit.Value);
        }
        
        var intensityByte = Convert.ToByte(intensity);
        return intensityByte;
    }
}