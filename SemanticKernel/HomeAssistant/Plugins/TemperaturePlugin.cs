using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Plugins;

using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

public class TemperaturePlugin
{
    private readonly ILogger<TemperaturePlugin> _logger;

    public TemperaturePlugin(ILogger<TemperaturePlugin> logger)
    {
        _logger = logger;
    }

    // Mock data for the rooms and their temperatures
    private readonly List<RoomTemperatureModel> _rooms =
    [
        new RoomTemperatureModel { Id = 1, Name = "Living Room", Temperature = 22.5 },
        new RoomTemperatureModel { Id = 2, Name = "Bedroom", Temperature = 19.0 },
        new RoomTemperatureModel { Id = 3, Name = "Bathroom", Temperature = 17.0 },
        new RoomTemperatureModel { Id = 4, Name = "Kitchen", Temperature = 24.0 }
    ];

    [KernelFunction("get_temperatures")]
    [Description("Gets a list of rooms and their current temperatures")]
    [return: Description("An array of room temperatures")]
    public async Task<List<RoomTemperatureModel>> GetTemperaturesAsync()
    {
        _logger.LogInformation("### GetTemperatures");

        return _rooms;
    }

    [KernelFunction("set_temperature")]
    [Description("Sets the temperature of a specified room")]
    [return: Description("The updated temperature of the room; will return null if the room does not exist")]
    public async Task<RoomTemperatureModel> SetTemperatureAsync(int id, double temperature)
    {
        _logger.LogInformation("### SetTemperature id: {Id} temperature: {Temperature}", id, temperature);
        var room = _rooms.FirstOrDefault(r => r.Id == id);

        if (room == null)
        {
            return null;
        }

        // Update the room with the new temperature
        room.Temperature = temperature;

        return room;
    }
}

public class RoomTemperatureModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("temperature")] public double Temperature { get; set; }
}