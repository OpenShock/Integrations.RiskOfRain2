using OpenShock.Integrations.RiskOfRain2.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;
using RoR2;
using UnityEngine;
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

        // If we recently died don't send any damage feedback so as not to interrupt death feedback
        if (Time.realtimeSinceStartupAsDouble - _lastDeathTime < 1.0)
        {
            if (_settingEnableVerboseLogging.Value)
                Logger.LogDebug("Not sending damage feedback due to recent death");
            return;
        }
        
        var intensityByte = CalculateIntensity(receivedDamage);
        
        if (_settingEnableVerboseLogging.Value)
            Logger.LogDebug($"ActionTimerElapsed - Damage: {receivedDamage:0.00} Intensity: {intensityByte}");

        ControlShockersFnf(_settingOnDamageMode.Value, intensityByte, (ushort)_settingOnDamageDuration.Value);
    }

    private void ClientOnDamage(DamageDealtMessage damageMessage)
    {
        if (_localUserCharacterBody == null || damageMessage.victim == null) return;
        if (damageMessage.victim != _localUserCharacterBody.gameObject) return;
        
        if (_settingEnableVerboseLogging.Value)
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

        if (_settingEnableVerboseLogging.Value)
            Logger.LogDebug($"Calculating Intensity - MaxHp: {playerMaxHp:0.00} CurrentHp: {playerCurrentHp:0.00} Damage: {damage:0.00} Behaviour: {onDamageBehaviour}");
        
        if (onDamageBehaviour is DamageBehaviour.LowHp or DamageBehaviour.DamagePercent)
        {
            float scaled = onDamageBehaviour switch
            {
                DamageBehaviour.LowHp => 1.0f - (playerCurrentHp / playerMaxHp),
                DamageBehaviour.DamagePercent => damage / playerMaxHp,
                _ => throw new Exception("This is unreachable.")
            };
            intensity = MathUtils.Lerp(0, _settingOnDamageIntensityLimit.Value, MathUtils.Saturate(scaled));
            if (_settingEnableVerboseLogging.Value)
                Logger.LogDebug($"Calculated Intensity - CurrentHp: {playerCurrentHp:0.00} / {playerMaxHp:0.00} Damage: {damage:0.00} Scaled: {scaled:0.0000} Intensity: {intensity:0.00}");
        }
        else // Only other option is DamageAbsolute
        {
            intensity = Math.Clamp(damage, 0, _settingOnDamageIntensityLimit.Value);
        }
        
        var intensityByte = Convert.ToByte(intensity);
        if (_settingEnableVerboseLogging.Value) Logger.LogDebug($"Final Intensity Byte: {intensityByte}");
        return intensityByte;
    }
}