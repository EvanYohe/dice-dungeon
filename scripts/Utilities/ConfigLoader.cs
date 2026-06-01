using System.IO;
using Godot;

namespace DiceDungeon.scripts.Utilities;

public partial class ConfigLoader : Node {
    
    private static ConfigLoader Instance { get; set; }
    private static readonly string ConfigDirectory = (string)ProjectSettings.GetSetting("global/Config_Directory");
    private static readonly string GlobalConfigDirectory = ProjectSettings.GlobalizePath(ConfigDirectory);
    private static readonly string ConfigFilePath = (string)ProjectSettings.GetSetting("global/Config_Directory") + "/config.json";
    private static readonly string GlobalConfigFile = ProjectSettings.GlobalizePath(ConfigFilePath);

    public override void _Ready() {
        
        Instance = this;
        CheckIfDirectoryExists();
        CheckIfFileExists();
        // subscribe to signals for config menu changes
    }

    public override void _ExitTree() {
        
        // unsubscribe from signals for config menu changes AFTER finished writing
        if (Instance == this) {
            Instance = null;
        }
    }

    private static void ReadConfigData() {
        
    }

    private static void WriteConfigData() {
        
    }

    private static void CheckIfDirectoryExists() {
        
        if (!Directory.Exists(GlobalConfigDirectory)) {
            GD.Print($"Creating Config directory: {GlobalConfigDirectory}");
            Directory.CreateDirectory(GlobalConfigDirectory);
        } else {
            GD.Print($"Config Directory found: {GlobalConfigDirectory}");
        }
    }

    private static void CheckIfFileExists() {
        
        if (!File.Exists(GlobalConfigFile)) {
            GD.Print($"Creating Config file: {GlobalConfigFile}");
            File.Create(GlobalConfigFile);
        } else {
            GD.Print($"Config file found: {GlobalConfigFile}");
        }
    }
}