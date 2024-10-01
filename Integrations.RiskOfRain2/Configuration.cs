using BepInEx.Configuration;
using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using OpenShock.Integrations.RiskOfRain2.Models;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
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
    private ConfigEntry<DamageBehaviour> _settingOnDamageBehaviour = null!;

    private ConfigEntry<int> _settingOnDamageDuration = null!;
    private ConfigEntry<int> _settingOnDamageIntensityLimit = null!;
    
    private void SetupConfiguration()
    {
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
        _settingOnDamageBehaviour = Config.Bind<DamageBehaviour>("OnDamage", "Behaviour", DamageBehaviour.LowHp, 
            "How the intensity is calculated. LowHp = Higher intensity the lower on HP you are; DamagePercentage = Intensity correlates to how much health you have lost of your max HP when damaged. DamageAbsolute = Intensity is equal to how much damage you recevied");

        _settingOnDamageDuration = Config.Bind<int>("OnDamage", "Duration", 100,
            "The duration of the shocker when the player takes damage");
        _settingOnDamageIntensityLimit = Config.Bind<int>("OnDamage", "IntensityLimit", 25,
            "Intensity limit for the shocker when the player takes damage");
    }

    private void RegisterConfigEvents()
    {
        _openShockServer.SettingChanged += UpdateApiClient;
        _openShockApiToken.SettingChanged += UpdateApiClient;
        _shockers.SettingChanged += ShockersOnSettingChanged;
    }
    
    private void UpdateApiClient(object sender, EventArgs e)
    {
        SetupApiClient();
    }
    
    private void ShockersOnSettingChanged(object sender, EventArgs e)
    {
        SetupShockers();
    }
}