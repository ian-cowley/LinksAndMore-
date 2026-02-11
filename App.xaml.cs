using System.Windows;
using LinksAndMore.Services;

namespace LinksAndMore;

public partial class App : Application
{
    public static IDataService DataService { get; } = new DataService();
    public static ISecurityService SecurityService { get; } = new SecurityService();
    public static IBiometricService BiometricService { get; } = new BiometricService();

    protected override async void OnStartup(StartupEventArgs e)
    {
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
