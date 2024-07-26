using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace HomeAssistant.Plugins;
using System.ComponentModel;
using Microsoft.SemanticKernel;
public class SmokeDetectorPlugin
{
    private readonly ILogger<LightsPlugin> _logger;

    public SmokeDetectorPlugin(ILogger<LightsPlugin> logger)
    {
        _logger = logger;
    }
    
    // Mock data for the smoke detectors
    private readonly List<SmokeDetectorModel> _smokeDetectors = new()
    {
        new SmokeDetectorModel { Id = 1, Name = "Living Room Smoke Detector", IsSmokeDetected = false },
        new SmokeDetectorModel { Id = 2, Name = "Kitchen Smoke Detector", IsSmokeDetected = false },
        new SmokeDetectorModel { Id = 3, Name = "Garage Smoke Detector", IsSmokeDetected = false }
    };

    [KernelFunction("get_smoke_detectors")]
    [Description("Gets a list of smoke detectors and their current state")]
    [return: Description("An array of smoke detectors")]
    public async Task<List<SmokeDetectorModel>> GetSmokeDetectorsAsync()
    {
        return _smokeDetectors;
    }

    [KernelFunction("detect_smoke")]
    [Description("Simulates smoke detection in the specified smoke detector")]
    [return: Description("The updated state of the smoke detector; will return null if the detector does not exist")]
    public async Task<SmokeDetectorModel> DetectSmokeAsync(int id, bool isSmokeDetected)
    {
        var detector = _smokeDetectors.FirstOrDefault(detector => detector.Id == id);
        if (detector == null)
        {
            return null;
        }
        // Update the detector with the new smoke detection state
        detector.IsSmokeDetected = isSmokeDetected;
        return detector;
    }
}

public class SmokeDetectorModel
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("is_smoke_detected")] public bool? IsSmokeDetected { get; set; }
}

