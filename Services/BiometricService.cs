using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace LinksAndMore.Services;

public interface IBiometricService
{
    Task<bool> AuthenticateUserAsync(string message);
}

public class BiometricService : IBiometricService
{
    public async Task<bool> AuthenticateUserAsync(string message)
    {
        try
        {
            // Check if biometric authentication is available
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (availability != UserConsentVerifierAvailability.Available)
            {
                // Biometrics not available, falling back to basic result (or could prompt for app password)
                // For now, we assume user should have some unlock method if biometrics are unavailable
                return true; 
            }

            // Request user consent
            var result = await UserConsentVerifier.RequestVerificationAsync(message);
            return result == UserConsentVerificationResult.Verified;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
