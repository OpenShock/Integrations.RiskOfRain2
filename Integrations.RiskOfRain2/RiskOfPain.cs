using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;

namespace OpenShock.Integrations.RiskOfRain2;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, "RiskOfPain", MyPluginInfo.PLUGIN_VERSION)]
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
[BepInDependency("com.rune580.riskofoptions")]
[BepInDependency(R2API.R2API.PluginGUID)]
public sealed partial class RiskOfPain : BaseUnityPlugin
{
    public static RiskOfPain Instance { get; private set; } = null!; // Singleton
    public static ManualLogSource ModLogger { get; private set; } = null!; // Mod Logger for e.g. LucTask
    
    private void Awake()
    {
        Logger.LogDebug("RiskOfPain loading...");

        Instance = this;
        ModLogger = Logger;

        _actionTimer = new Timer(ActionTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        
        SetupConfiguration();
        RegisterConfigEvents();
        
        SetupApiClient();
        SetupShockers();

        SetupRiskOfOptionsMenu();
        
        Logger.LogDebug("RiskOfPain loaded");
    }
    
    private void OnEnable()
    {
        // R2API events
        CharacterBody.onBodyStartGlobal += OnCharacterBodyStart;
        CharacterBody.onBodyDestroyGlobal += OnCharacterBodyDestroy;
        GlobalEventManager.onClientDamageNotified += ClientOnDamage;
        Logger.LogDebug("Events registered");
    }

    private void OnDisable()
    {
        // R2API events
        CharacterBody.onBodyStartGlobal -= OnCharacterBodyStart;
        CharacterBody.onBodyDestroyGlobal -= OnCharacterBodyDestroy;
        GlobalEventManager.onClientDamageNotified -= ClientOnDamage;
        Logger.LogDebug("Events unregistered");
    }
    
}