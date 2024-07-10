using Dalamud.Configuration;

namespace XIVJitterFix;

public class Config : IPluginConfiguration
{
    public float JitterMultiplier { get; set; } = 0.6f;
    public bool SetDownscaleBuffers { get; set; } = false;
    public byte DownscaleBuffers { get; set; } = 1;
    public int Version { get; set; } = 0;
}
