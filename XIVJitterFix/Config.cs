using Dalamud.Configuration;

namespace XIVJitterFix;

public class Config : IPluginConfiguration
{
    public float JitterMultiplier { get; set; } = 0.6f;
    public int Version { get; set; } = 0;
}
