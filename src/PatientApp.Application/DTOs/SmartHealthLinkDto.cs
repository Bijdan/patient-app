using System.Text.Json.Serialization;

namespace PatientApp.Application.DTOs;

public class SmartHealthLinkDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("flag")]
    public string Flag { get; set; } = null!;

    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    [JsonPropertyName("exp")]
    public long Exp { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = null!;
}
