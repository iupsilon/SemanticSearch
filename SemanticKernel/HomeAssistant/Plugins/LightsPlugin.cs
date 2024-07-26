using System.Reflection;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Plugins;

using System.ComponentModel;
using Microsoft.SemanticKernel;

public class LightsPlugin
{
    private readonly ILogger<LightsPlugin> _logger;

    public LightsPlugin(ILogger<LightsPlugin> logger)
    {
        _logger = logger;
    }

    // Mock data for the lights
    private readonly List<LightModel> _lights = new()
    {
        new LightModel { Id = 1, Name = "Table Lamp", IsOn = false },
        new LightModel { Id = 2, Name = "Porch light", IsOn = false },
        new LightModel { Id = 3, Name = "Living room Chandelier", IsOn = true },
        new LightModel { Id = 4, Name = "Backyard lamp", IsOn = false },
        new LightModel { Id = 5, Name = "Bathroom lamp", IsOn = false },
        new LightModel { Id = 6, Name = "Bathroom Chandelier", IsOn = false },
        new LightModel { Id = 7, Name = "Kitchen Chandelier", IsOn = false },
        new LightModel { Id = 8, Name = "Kitchen Oven light", IsOn = false },
        new LightModel { Id = 9, Name = "Bedroom side table light", IsOn = false },
        new LightModel { Id = 10, Name = "Bedroom Chandelier", IsOn = false },
    };

    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    [return: Description("An array of lights")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        _logger.LogInformation("### GetLights");
        return _lights;
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    [return: Description("The updated state of the light; will return null if the light does not exist")]
    public async Task<LightModel> ChangeStateAsync(int id, bool isOn)
    {
        _logger.LogInformation("### ChangeState id: {Id} isOn: {IsOn}", id, isOn);
        var light = _lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
        {
            return null;
        }

        // Update the light with the new state
        light.IsOn = isOn;

        return light;
    }
}

public class LightModel
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("is_on")] public bool? IsOn { get; set; }
}