using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace HomeAssistant.Plugins;

public class FirePreventionPlugin
{
    // Mock data for the fire prevention devices
    private readonly List<FirePreventionDeviceModel> _devices = new()
    {
        new FirePreventionDeviceModel { Id = 1, Name = "Living Room Smoke Detector", IsActive = false },
        new FirePreventionDeviceModel { Id = 2, Name = "Kitchen Smoke Detector", IsActive = true },
        new FirePreventionDeviceModel { Id = 3, Name = "Garage Smoke Detector", IsActive = true }
    };

    [KernelFunction("get_fire_prevention_devices")]
    [Description("Gets a list of fire prevention devices and their current state")]
    [return: Description("An array of fire prevention devices")]
    public async Task<List<FirePreventionDeviceModel>> GetFirePreventionDevicesAsync()
    {
        return _devices;
    }

    [KernelFunction("change_device_state")]
    [Description("Changes the state of the fire prevention device")]
    [return: Description("The updated state of the fire prevention device; will return null if the device does not exist")]
    public async Task<FirePreventionDeviceModel> ChangeDeviceStateAsync(int id, bool isActive)
    {
        var device = _devices.FirstOrDefault(device => device.Id == id);
        if (device == null)
        {
            return null;
        }

        // Update the device with the new state
        device.IsActive = isActive;
        return device;
    }

    public class FirePreventionDeviceModel
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("is_active")] public bool? IsActive { get; set; }
    }
}