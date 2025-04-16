namespace SecureBankingSystem.Services
{
    public interface ILocationService
    {
        Task<string> GetLocationFromIP(string ipAddress);
        Task<double> CalculateGeoDistance(string location1, string location2);
        Task<bool> IsLocationCommonForUser(string userId, string location);
        Task AddCommonLocationForUser(string userId, string location);
    }
}