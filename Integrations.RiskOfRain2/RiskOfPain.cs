using BepInEx;
using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;
using RoR2;

namespace OpenShock.Integrations.RiskOfRain2;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, "RiskOfPain", MyPluginInfo.PLUGIN_VERSION)]
public class RiskOfPain : BaseUnityPlugin
{
    private const string ActualVersion = "1.0.0";
    private readonly Version _version = new Version(ActualVersion);

    private void Awake()
    {
        Logger.LogDebug("RiskOfPain loaded...");
        
    }

    private void OnEnable()
    {
        CharacterBody.onBodyStartGlobal += OnCharacterBodyStart;
        CharacterBody.onBodyDestroyGlobal += OnCharacterBodyDestroy;
        GlobalEventManager.onClientDamageNotified += ClientOnDamage;
        Logger.LogDebug("Events registered");

        var openShockApiClient = new OpenShockApi.OpenShockApi(new Uri("https://api.openshock.app/"), ""); 
        
        
        Task.Run(async () =>
        {
            try
            {
                Logger.LogDebug("Sending control request");
                var shocks = new List<Control>();
                shocks.Add(new Control()
                {
                    Id = Guid.Parse("d9267ca6-d69b-4b7a-b482-c455f75a4408"),
                    Duration = 1000,
                    Intensity = (byte)Math.Clamp(50, 0, 100),
                    Type = ControlType.Vibrate
                });
                await openShockApiClient.Control(shocks);

                Logger.LogDebug("control response");
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        });
        
        Logger.LogDebug("OpenShockApiClient registered");
    }

    private void OnDisable()
    {
        CharacterBody.onBodyStartGlobal -= OnCharacterBodyStart;
        CharacterBody.onBodyDestroyGlobal -= OnCharacterBodyDestroy;
        GlobalEventManager.onClientDamageNotified -= ClientOnDamage;
        Logger.LogDebug("Events unregistered");
    }

    private void OnCharacterBodyDestroy(CharacterBody characterBody)
    {
        if (!characterBody.isPlayerControlled) return; // Optimize logic

        if (_localUserCharacterBody != characterBody) return;

        _localUserCharacterBody = null;
    }

    private CharacterBody? _localUserCharacterBody = null;

    private void OnCharacterBodyStart(CharacterBody characterBody)
    {
        if (!characterBody.isPlayerControlled) return;
        var networkUser = Util.LookUpBodyNetworkUser(characterBody);
        if (networkUser == null || !networkUser.isLocalPlayer) return;

        _localUserCharacterBody = characterBody;

        Logger.LogDebug($"Local user found. {networkUser.userName}");
    }

    private void ClientOnDamage(DamageDealtMessage damageMessage)
    {
        
        try
        {
            if (_localUserCharacterBody == null) return;
            if (damageMessage.victim != _localUserCharacterBody.gameObject) return;
            
            Logger.LogError("YOOO WE RECEIVED DAMAGE TO THE RISK OF RAIN! - " + damageMessage.victim.name + " - " +
                            damageMessage.damage);
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }
    }
}