using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace XIVJitterFix;

public class MainWindow : Window
{
    public MainWindow() : base("XIVJitterFix Info###XivJitterFix")
    {
        Size = new System.Numerics.Vector2(500, 400);
    }

    private void WrapText(string text)
    {
        ImGui.PushTextWrapPos(0);
        ImGui.TextUnformatted(text);
        ImGui.PopTextWrapPos();
    }

    public override void Draw()
    {
        WrapText("This plugin is active automatically, there is no configuration. To turn off the functionality, just disable the plugin.");
        ImGui.Separator();
        WrapText("Warning: Do not use this plugin if you don't use DLAA, DLSS or TSCMAA+jitter.");
        ImGui.Separator();
        WrapText("What is jitter and why do I want it?");
        WrapText("Jittering the camera is an essential part of temporal antialiasing solutions like the aforementioned ones. " +
            "The micromovements introduced by jitter resolve aliased and more static edges " +
            "and especially help with alpha transparencies like foliage. Whenever possible, jitter should be used.");
        WrapText("This normally happens automatically and so it does in this game, but for some reason SE decided to not jitter during cutscenes, GPose and NPC dialogue.");
        WrapText("You may have noticed that the antialiasing gets worse once you do one of the mentioned tasks. This is because jitter gets disabled.");
        WrapText("From my testing I have found no reason why jitter should ever be disabled and it improves the image quality especially in those situations.");
    }
}
