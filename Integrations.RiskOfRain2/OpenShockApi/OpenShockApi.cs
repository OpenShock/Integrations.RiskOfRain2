using System.Text;
using BepInEx.Logging;
using Newtonsoft.Json;
using OpenShock.Integrations.LethalCompany.OpenShockApi.Models;
using OpenShock.Integrations.RiskOfRain2.OpenShockApi.Models;

namespace OpenShock.Integrations.RiskOfRain2.OpenShockApi;

public class OpenShockApi
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("OpenShockApi");
    private readonly HttpClient _httpClient;
    
    public OpenShockApi(Uri server, string apiToken)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = server
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"OpenShock.Integrations.RiskOfRain2/{MyPluginInfo.PLUGIN_VERSION}");
        _httpClient.DefaultRequestHeaders.Add("OpenShockToken", apiToken);
    }

    public async Task Control(IEnumerable<Control> shocks)
    {
        Logger.LogInfo("Sending control request to OpenShock API");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/2/shockers/control")
        {
            Content = new StringContent(JsonConvert.SerializeObject(new ControlRequest
            {
                Shocks = shocks,
                CustomName = "Integrations.RiskOfRain2"
            }), Encoding.UTF8, "application/json")
        };
        var response = await _httpClient.SendAsync(requestMessage);
        
        if (!response.IsSuccessStatusCode) Logger.LogError($"Failed to send control request to OpenShock API [{response.StatusCode}]");
    }
}