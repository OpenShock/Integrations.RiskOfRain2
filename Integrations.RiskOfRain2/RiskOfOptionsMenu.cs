using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace OpenShock.Integrations.RiskOfRain2;

public sealed partial class RiskOfPain
{
    private void SetupRiskOfOptionsMenu()
    {
        ModSettingsManager.AddOption(new StringInputFieldOption(_openShockServer));
        ModSettingsManager.AddOption(new StringInputFieldOption(_openShockApiToken, new InputFieldConfig()
        {
            submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit
        }));
        ModSettingsManager.AddOption(new StringInputFieldOption(_shockers, new InputFieldConfig()
        {
            submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit
        }));

        ModSettingsManager.AddOption(
            new GenericButtonOption(
                "TestShockers",
                "OpenShock",
                "Will vibrate all configured shockers at 25% and 1s",
                "Test Shockers",
                () =>
                {
                    ControlShockersFnf(ControlType.Vibrate, 25, 1000);
                }
            ));

        ModSettingsManager.AddOption(new CheckBoxOption(_settingOnDamageEnabled));
        ModSettingsManager.AddOption(new ChoiceOption(_settingOnDamageMode));
        ModSettingsManager.AddOption(new ChoiceOption(_settingOnDamageBehaviour));
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
    }
}