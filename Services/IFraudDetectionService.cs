using SecureBankingSystem.Models;

namespace SecureBankingSystem.Services
{
    public interface IFraudDetectionService
    {
        Task<bool> IsLoginSuspicious(string userId, string ipAddress, string userAgent, string location);
        Task RecordLoginAttempt(string userId, string ipAddress, string userAgent, string location, bool successful);
        Task<IEnumerable<FraudAlert>> GetUnresolvedAlerts(string userId);

        // Add these methods to the interface
        Task<FraudAlert> GetAlertById(int alertId);
        Task ResolveAlert(int alertId, string resolution);
    }
}