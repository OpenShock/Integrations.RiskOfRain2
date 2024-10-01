using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;
using R2API.Utils;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;

namespace OpenShock.Integrations.RiskOfRain2;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, "RiskOfPain", MyPluginInfo.PLUGIN_VERSION)]
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
[BepInDependency("com.rune580.riskofoptions")]
public class RiskOfPain : BaseUnityPlugin
{
    public static RiskOfPain Instance { get; private set; } = null!;
    public static ManualLogSource ModLogger { get; private set; } = null!;

    private OpenShockApi.OpenShockApi? _openShockApiClient = null;
    private IList<Guid> _shockersList = null!;

    // OpenShock Server
    private ConfigEntry<string> _openShockServer = null!;
    private ConfigEntry<string> _openShockApiToken = null!;
    private ConfigEntry<string> _shockers = null!;

    // Settings

    // OnDeath
    private ConfigEntry<byte> _settingOnDeathIntensity = null!;
    private ConfigEntry<ushort> _settingOnDeathDuration = null!;

    // OnDamage
    private ConfigEntry<bool> _settingOnDamageEnabled = null!;
    private ConfigEntry<ControlType> _settingOnDamageMode = null!;

    private ConfigEntry<int> _settingOnDamageDuration = null!;
    private ConfigEntry<int> _settingOnDamageIntensityLimit = null!;

    private void Awake()
    {
        Logger.LogDebug("RiskOfPain loading...");

        Instance = this;
        ModLogger = Logger;

        // Config

        // OpenShock Server
        _openShockServer = Config.Bind<string>("OpenShock", "Server", "https://api.openshock.app",
            "The URL of the OpenShock backend");
        _openShockApiToken = Config.Bind<string>("OpenShock", "ApiToken", "",
            "API token for authentication, can be found under API Tokens on the OpenShock dashboard (https://shocklink.net/#/dashboard/tokens)");
        _shockers = Config.Bind<string>("Shockers", "Shockers", "comma,seperated,list,of,shocker,ids",
            "A list of shocker IDs to use within the mod. Comma seperated.");


        // Settings

        // OnDeath
        _settingOnDeathIntensity = Config.Bind<byte>("OnDeath", "Intensity", 25,
            "The intensity of the shocker when the player dies");
        _settingOnDeathDuration = Config.Bind<ushort>("OnDeath", "Duration", 1000,
            "The duration of the shocker when the player dies");

        // OnDamage

        _settingOnDamageEnabled = Config.Bind<bool>("OnDamage", "Enabled", true, "Enables on damage");
        _settingOnDamageMode = Config.Bind<ControlType>("OnDamage", "Mode", ControlType.Shock,
            "The action that happens when you take damage");

        _settingOnDamageDuration = Config.Bind<int>("OnDamage", "Duration", 100,
            "The duration of the shocker when the player takes damage");
        _settingOnDamageIntensityLimit = Config.Bind<int>("OnDamage", "IntensityLimit", 25,
            "Intensity limit for the shocker when the player takes damage");

        _openShockServer.SettingChanged += UpdateApiClient;
        _openShockApiToken.SettingChanged += UpdateApiClient;
        _shockers.SettingChanged += ShockersOnSettingChanged;

        SetupApiClient();
        SetupShockers();

        ModSettingsManager.AddOption(new StringInputFieldOption(_openShockServer));
        ModSettingsManager.AddOption(new StringInputFieldOption(_openShockApiToken, new InputFieldConfig()
        {
            submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit
        }));
        ModSettingsManager.AddOption(new StringInputFieldOption(_shockers, new InputFieldConfig()
        {
            submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit
        }));

        ModSettingsManager.AddOption(new CheckBoxOption(_settingOnDamageEnabled));
        ModSettingsManager.AddOption(new ChoiceOption(_settingOnDamageMode));
        ModSettingsManager.AddOption(new IntSliderOption(_settingOnDamageDuration, new IntSliderConfig
        {
            min = 300,
            max = 30_000
        }));
        ModSettingsManager.AddOption(new IntSliderOption(_settingOnDamageIntensityLimit, new IntSliderConfig
        {
            min = 1,
            max = 100
        }));


        Logger.LogDebug("RiskOfPain loaded");
    }

    private void ShockersOnSettingChanged(object sender, EventArgs e)
    {
        SetupShockers();
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

    private void UpdateApiClient(object sender, EventArgs e)
    {
        SetupApiClient();
    }

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

    private void OnEnable()
    {
        CharacterBody.onBodyStartGlobal += OnCharacterBodyStart;
        CharacterBody.onBodyDestroyGlobal += OnCharacterBodyDestroy;
        GlobalEventManager.onClientDamageNotified += ClientOnDamage;
        Logger.LogDebug("Events registered");
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
        if (_localUserCharacterBody == null) return;
        if (damageMessage.victim != _localUserCharacterBody.gameObject) return;

        Logger.LogDebug($"Player received damage: {damageMessage.damage}");


        LucTask.Run(async () =>
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
                    Duration = (ushort)_settingOnDamageDuration.Value,
                    Intensity = (byte)_settingOnDamageIntensityLimit.Value,
                    Type = _settingOnDamageMode.Value
                });

            await _openShockApiClient.Control(controls);

            Logger.LogDebug("");
        });
    }
}