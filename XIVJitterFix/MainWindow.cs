using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;

namespace XIVJitterFix;

public class MainWindow : Window
{
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly Config pluginConfig;

    public MainWindow(Config config, IDalamudPluginInterface dalamudPluginInterface) : base("XIVJitterFix Info###XivJitterFix")
    {
        Size = new System.Numerics.Vector2(500, 400);
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.pluginConfig = config;
    }

    private void WrapText(string text)
    {
        ImGui.PushTextWrapPos(0);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    public override void Draw()
    {
        WrapText("This plugin is active automatically. To turn off the functionality, just disable the plugin.");
        ImGui.Separator();
        WrapText("Warning: Do not use this plugin if you don't use DLAA, DLSS or TSCMAA+jitter.");
        ImGui.Separator();
        var help = ImRaii.TreeNode("What is jitter and why do I want it?");
        if (help)
        {

            WrapText("Jittering the camera is an essential part of temporal antialiasing solutions like the aforementioned ones. " +
                "The micromovements introduced by jitter resolve aliased and more static edges " +
                "and especially help with alpha transparencies like foliage. Whenever possible, jitter should be used.");
            WrapText("This normally happens automatically and so it does in this game, but for some reason SE decided to not jitter during cutscenes, GPose and NPC dialogue.");
            WrapText("You may have noticed that the antialiasing gets worse once you do one of the mentioned tasks. This is because jitter gets disabled.");
            WrapText("From my testing I have found no reason why jitter should ever be disabled and it improves the image quality especially in those situations.");
        }
        help.Dispose();

        ImGui.Separator();
        var expertConfig = ImRaii.TreeNode("Expert Config");
        if (expertConfig)
        {
            float jitterMultiplier = pluginConfig.JitterMultiplier;
            ImGui.SetNextItemWidth(150);
            bool configDirty = false;
            if (ImGui.SliderFloat("Jitter Multiplier", ref jitterMultiplier, 0.1f, 3.0f))
            {
                pluginConfig.JitterMultiplier = jitterMultiplier;
                configDirty = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset"))
            {
                pluginConfig.JitterMultiplier = 0.6f;
                configDirty = true;
            }
            WrapText("This multiplier steers how much jittering is done through the game. 0.6 is default. Play around and see what works best for you. " +
                "Sane values are probably somewhere between 0.5 and 1.5. If you need more precise values use Ctrl+Click into the slider. " +
                "Jitter is more noticeable with TSCMAA+jitter than DLAA.");

            bool setDownscaleBuffers = pluginConfig.SetDownscaleBuffers;
            if (ImGui.Checkbox("Override Downscaling Settings", ref setDownscaleBuffers))
            {
                pluginConfig.SetDownscaleBuffers = setDownscaleBuffers;
                configDirty = true;
            }
            using (ImRaii.Disabled(!setDownscaleBuffers))
            {
                using var _ = ImRaii.PushIndent(10f);

                bool ignoreDownscaling = pluginConfig.DownscaleBuffers == 0;
                if (ImGui.Checkbox("Ignore Downscaling", ref ignoreDownscaling))
                {
                    pluginConfig.DownscaleBuffers = (byte)(ignoreDownscaling ? 0 : 1);
                    configDirty = true;
                }
                WrapText("This appears to fix the bloom and depth of field buffers when running any downscaling. Turn this on if you use DLAA. Note: this will also force DLAA on always as no downscaling will be performed anymore.");
                if (configDirty)
                {
                    dalamudPluginInterface.SavePluginConfig(pluginConfig);
                }
            }
        }

        expertConfig.Dispose();
    }
}
