using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using System.Runtime.InteropServices;
using Dalamud.Interface.Windowing;

namespace XIVJitterFix;

public sealed class Plugin : IDalamudPlugin
{
    private readonly nint hookAddr;

    private readonly IPluginLog logger;
    private readonly IFramework framework;
    private readonly WindowSystem windowSystem;
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly MainWindow mainWindow;
    private readonly Config pluginConfig;

    public unsafe Plugin(IPluginLog logger, ISigScanner sigScanner,
        IFramework framework, IDalamudPluginInterface dalamudPluginInterface)
    {
        this.logger = logger;
        this.framework = framework;
        this.dalamudPluginInterface = dalamudPluginInterface;
        windowSystem = new("XIVJitterFix");
        pluginConfig = dalamudPluginInterface.GetPluginConfig() as Config ?? new();
        mainWindow = new MainWindow(pluginConfig, dalamudPluginInterface);
        windowSystem.AddWindow(mainWindow);

        hookAddr = sigScanner.GetStaticAddressFromSig("48 89 05 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ??");

        framework.Update += Framework_Update;
        dalamudPluginInterface.UiBuilder.OpenConfigUi += UiBuilder_OpenConfigUi;
        dalamudPluginInterface.UiBuilder.Draw += UiBuilder_Draw;
    }

    private void UiBuilder_Draw()
    {
        windowSystem.Draw();
    }

    private void UiBuilder_OpenConfigUi()
    {
        mainWindow.Toggle();
    }

    private unsafe void Framework_Update(IFramework framework)
    {
        if (hookAddr == nint.Zero) return;
        var addr1 = *(nint*)hookAddr;
        if (addr1 == nint.Zero) return;

        ProbablySomeGraphicsConfig* config = (ProbablySomeGraphicsConfig*)addr1;

        var prevValue1 = config->NpcGposeJitter;
        var prevValue2 = config->CutsceneJitter;

        if (prevValue1 != 1 || prevValue2 != 1)
        {
            config->NpcGposeJitter = 1;
            config->CutsceneJitter = 1;
            logger.Verbose("Detected change NpcGpose {0} / Cutscene {1} -> NpcGpose {2} / Cutscene {3}", prevValue1, prevValue2, config->NpcGposeJitter, config->CutsceneJitter);
        }

        if (pluginConfig.JitterMultiplier != config->JitterMultiplier)
        {
            logger.Verbose("Detected change JitterMult current {0} -> desired {1}", config->JitterMultiplier, pluginConfig.JitterMultiplier);

            config->JitterMultiplier = pluginConfig.JitterMultiplier;
        }

        if (pluginConfig.DownscaleBuffers != config->DownscaleBuffers)
        {
            logger.Verbose("Detected change DownscaleBuffers current {0} -> desired {1}", config->DownscaleBuffers, pluginConfig.DownscaleBuffers);

            config->DownscaleBuffers = pluginConfig.DownscaleBuffers;
        }
    }

    public void Dispose()
    {
        windowSystem.RemoveAllWindows();

        dalamudPluginInterface.UiBuilder.OpenConfigUi -= UiBuilder_OpenConfigUi;
        dalamudPluginInterface.UiBuilder.Draw -= UiBuilder_Draw;

        framework.Update -= Framework_Update;
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct ProbablySomeGraphicsConfig
    {
        [FieldOffset(0x19)] public byte NpcGposeJitter;
        [FieldOffset(0x1a)] public byte CutsceneJitter;

        /// <summary>
        /// 0 = nothing, 1 = fxaa, 2 = tscmaa+jitter, 3 = tscmaa
        /// </summary>
        [FieldOffset(0x2c)] public byte AntiAliasingMode; 

        /// <summary>
        /// 0 = off, 1 = on
        /// </summary>
        [FieldOffset(0x44)] public byte DynamicResolution; 

        /// <summary>
        /// seems like it affects dof/bloom shaders when running dlss or dynamic res
        /// </summary>
        [FieldOffset(0x45)] public byte DownscaleBuffers; 

        /// <summary>
        /// FSR = 1, DLSS = 2
        /// </summary>
        [FieldOffset(0x54)] public byte DlssFsrSwitch;
        [FieldOffset(0x64)] public float JitterMultiplier;
    }
}
