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

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Must be set before any UI is created so Windows properly
        // associates the pinned taskbar shortcut with this app's icon
        SetCurrentProcessExplicitAppUserModelID("IanCowley.LinksAndMore");

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
