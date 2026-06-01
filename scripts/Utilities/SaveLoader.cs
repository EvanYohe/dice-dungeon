using System.IO;
using Godot;

namespace DiceDungeon.scripts.Utilities;

public partial class SaveLoader : Node {
    
    private static SaveLoader Instance { get; set; }
    private static readonly string SaveDirectory = (string)ProjectSettings.GetSetting("global/Save_Directory");
    private static readonly string GlobalSaveDirectory = ProjectSettings.GlobalizePath(SaveDirectory);
    private static readonly string SaveFilePath = (string)ProjectSettings.GetSetting("global/Save_Directory") + "/save.json";
    private static readonly string GlobalSaveFile = ProjectSettings.GlobalizePath(SaveFilePath);

    public override void _Ready() {
        
        Instance = this;
        CheckIfDirectoryExists();
        CheckIfFileExists();
        // subscribe to signals for save file read/write
    }

    public override void _ExitTree() {
        
        // unsubscribe from signals for save file read/write AFTER finished writing
        if (Instance == this) {
            Instance = null;
        }
    }

    private static void ReadSaveData() {
        
    }

    private static void WriteSaveData() {
        
    }

    private static void CheckIfFileExists() {
        
        if (!File.Exists(GlobalSaveFile)) {
            GD.Print($"Creating Save file: {GlobalSaveFile}");
            File.Create(GlobalSaveFile);
        } else {
            GD.Print($"Save file found: {GlobalSaveFile}");
        }
    }

    private static void CheckIfDirectoryExists() {
        
        if (!Directory.Exists(GlobalSaveDirectory)) {
            GD.Print($"Creating Save directory: {GlobalSaveDirectory}");
            Directory.CreateDirectory(GlobalSaveDirectory);
        } else {
            GD.Print($"Save Directory found: {GlobalSaveDirectory}");
        }
    }
}