using System;
using System.IO;
using System.Threading.Tasks;
using Godot;

namespace DiceDungeon.scripts.Utilities;

public partial class ErrorHandler : Node {

    [Signal]
    public delegate void ErrorReportedEventHandler(string message, string details);

    private static ErrorHandler Instance { get; set; }
    private static readonly string LogDirectory = (string)ProjectSettings.GetSetting("global/Error_Log_Directory");
    private static readonly string LogFilePath = (string)ProjectSettings.GetSetting("global/Error_Log_Directory") + "error.log";

    public override void _Ready() {
        
        Instance = this;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        CheckDirectoryExists();

        GD.Print("ErrorHandler initialized.");
    }

    public override void _ExitTree() {
        
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

        if (Instance == this) {
            Instance = null;
        }
    }

    public void ReportError(string message, Exception exception = null) {
        
        string details = exception?.ToString() ?? string.Empty;
        GD.PushError(string.IsNullOrWhiteSpace(details) ? message : $"{message}\n{details}");
        WriteErrorToLog(message, details);
        EmitSignal(SignalName.ErrorReported, message, details);
    }

    public void ReportWarning(string message) {
        
        GD.PushWarning(message);
        WriteErrorToLog($"WARNING: {message}", string.Empty);
    }

    public void ReportInfo(string message) {
        
        GD.Print(message);
        WriteErrorToLog($"INFO: {message}", string.Empty);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args) {
        
        Exception exception = args.ExceptionObject as Exception;
        string message = exception is null ? $"Unhandled exception: {args.ExceptionObject}" : "Unhandled exception occurred.";
        ReportError(message, exception);
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args) {
        
        ReportError("Unobserved task exception occurred.", args.Exception);
        args.SetObserved();
    }

    private static void CheckDirectoryExists() {
        
        string globalizedLogDirectory = ProjectSettings.GlobalizePath(LogDirectory);
        if (!Directory.Exists(globalizedLogDirectory)) {
            GD.Print($"Creating Error directory: {globalizedLogDirectory}");
            Directory.CreateDirectory(globalizedLogDirectory);
        }
        else {
            GD.Print($"Error Directory found: {globalizedLogDirectory}");
        }
    }

    private void WriteErrorToLog(string message, string details) {
        
        try {
            CheckDirectoryExists();
            string globalizedLogFilePath = ProjectSettings.GlobalizePath(LogFilePath);
            string logEntry = $"""
                               [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]
                               {message}
                               {details}

                               """;

            File.AppendAllText(globalizedLogFilePath, logEntry);
        } catch (Exception exception) {
            GD.PushError($"Failed to write error log: {exception}");
        }
    }
}