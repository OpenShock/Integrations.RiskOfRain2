using RoR2;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
    private CharacterBody? _localUserCharacterBody = null;

    private void OnCharacterBodyStart(CharacterBody characterBody)
    {
        if (!characterBody.isPlayerControlled) return;
        var networkUser = Util.LookUpBodyNetworkUser(characterBody);
        if (networkUser == null || !networkUser.isLocalPlayer) return;

        _localUserCharacterBody = characterBody;

        Logger.LogDebug($"Local user found. {networkUser.userName}");
    }
    
    private void OnCharacterBodyDestroy(CharacterBody characterBody)
    {
        if (!characterBody.isPlayerControlled) return; // Optimize logic

        if (_localUserCharacterBody != characterBody) return;

        _localUserCharacterBody = null;
        
        Logger.LogDebug("Local user destroyed");
    }
}