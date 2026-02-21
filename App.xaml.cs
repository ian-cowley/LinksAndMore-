using System.Runtime.InteropServices;
using System.Windows;
using LinksAndMore.Services;

namespace LinksAndMore;

public partial class App : Application
{
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID(
        [MarshalAs(UnmanagedType.LPWStr)] string appId);

    public static IDataService DataService { get; } = new DataService();
    public static ISecurityService SecurityService { get; } = new SecurityService();
    public static IBiometricService BiometricService { get; } = new BiometricService();
    public static ISemanticEngine SemanticEngine { get; } = new SemanticEngine();
    private static readonly string SettingsFilePath = System.IO.Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
        "LinksAndMore", "appsettings.txt");

    private static bool _isSemanticSearchEnabled;
    public static bool IsSemanticSearchEnabled 
    { 
        get => _isSemanticSearchEnabled;
        set 
        {
            _isSemanticSearchEnabled = value;
            try
            {
                System.IO.File.WriteAllText(SettingsFilePath, value.ToString());
            }
            catch { }
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Must be set before any UI is created so Windows properly
        // associates the pinned taskbar shortcut with this app's icon
        SetCurrentProcessExplicitAppUserModelID("IanCowley.LinksAndMore");

        try
        {
            if (System.IO.File.Exists(SettingsFilePath))
            {
                var txt = System.IO.File.ReadAllText(SettingsFilePath);
                if (bool.TryParse(txt, out bool b))
                {
                    _isSemanticSearchEnabled = b;
                }
            }
        }
        catch { }

        base.OnStartup(e);

        try
        {
            // One-time migration if file doesn't exist
            var existingData = await DataService.LoadDataAsync();
            
            if (!existingData.Any())
            {
                var migrated = MigrationService.GetDefaultData();
                await DataService.SaveDataAsync(migrated);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Startup Error: {ex.Message}");
        }
    }
}
