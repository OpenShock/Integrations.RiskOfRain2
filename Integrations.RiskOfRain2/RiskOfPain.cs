using BepInEx;
using Newtonsoft.Json;
using RoR2;

namespace Integrations.RiskOfRain2;

[BepInPlugin("OpenShock.Integrations.RiskOfRain2", "RiskOfPain", "1.0.0")]
public class RiskOfPain : BaseUnityPlugin
{
    private void OnEnable()
    {
        GlobalEventManager.onClientDamageNotified += ClientOnDamage;
    }

    private void OnDisable()
    {
        GlobalEventManager.onClientDamageNotified -= ClientOnDamage;
    }

    private void ClientOnDamage(DamageDealtMessage damageMessage)
    {
        
    }
}