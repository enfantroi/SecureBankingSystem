using Microsoft.EntityFrameworkCore;
using SecureBankingSystem.Data;
using SecureBankingSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureBankingSystem.Services
{
    public class FraudDetectionService : IFraudDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILocationService _locationService;

        public FraudDetectionService(ApplicationDbContext context, ILocationService locationService)
        {
            _context = context;
            _locationService = locationService;
        }

        public async Task<bool> IsLoginSuspicious(string userId, string ipAddress, string userAgent, string location)
        {
            var suspiciousFactors = new List<string>();

            // Check if location is common for this user
            bool isCommonLocation = await _locationService.IsLocationCommonForUser(userId, location);
            if (!isCommonLocation)
            {
                suspiciousFactors.Add("Unusual location");
            }

            // Check for multiple failed login attempts in the last hour
            var failedAttempts = await _context.LoginAttempts
                .Where(la => la.UserId == userId && !la.Successful && la.AttemptTime > DateTime.UtcNow.AddHours(-1))
                .CountAsync();

            if (failedAttempts >= 3)
            {
                suspiciousFactors.Add("Multiple failed attempts");
            }

            // Check for logins from multiple different locations in a short time
            var recentLocations = await _context.LoginAttempts
                .Where(la => la.UserId == userId && la.AttemptTime > DateTime.UtcNow.AddMinutes(-30))
                .Select(la => la.Location)
                .Distinct()
                .ToListAsync();

            if (recentLocations.Count > 1)
            {
                foreach (var recentLocation in recentLocations)
                {
                    double distance = await _locationService.CalculateGeoDistance(location, recentLocation);
                    // If distance is more than 100 miles and time difference is small, flag as suspicious
                    if (distance > 100)
                    {
                        suspiciousFactors.Add("Login from distant locations in short time period");
                        break;
                    }
                }
            }

            // If any suspicious factors were found, create an alert
            if (suspiciousFactors.Count > 0)
            {
                await CreateFraudAlert(userId, suspiciousFactors);
                return true;
            }

            return false;
        }

        public async Task RecordLoginAttempt(string userId, string ipAddress, string userAgent, string location, bool successful)
        {
            var loginAttempt = new LoginAttempt
            {
                UserId = userId,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Location = location,
                Successful = successful,
                AttemptTime = DateTime.UtcNow
            };

            // Check if this attempt is suspicious
            bool isSuspicious = await IsLoginSuspicious(userId, ipAddress, userAgent, location);
            loginAttempt.FlaggedAsSuspicious = isSuspicious;

            if (isSuspicious)
            {
                loginAttempt.SuspiciousFactors = "See fraud alerts for details";
            }

            _context.LoginAttempts.Add(loginAttempt);
            await _context.SaveChangesAsync();

            // If login was successful and location is not suspicious, add it to common locations
            if (successful && !isSuspicious)
            {
                await _locationService.AddCommonLocationForUser(userId, location);
            }
        }

        public async Task<IEnumerable<FraudAlert>> GetUnresolvedAlerts(string userId)
        {
            return await _context.FraudAlerts
                .Where(fa => fa.UserId == userId && !fa.Resolved)
                .ToListAsync();
        }

        private async Task CreateFraudAlert(string userId, List<string> suspiciousFactors)
        {
            var alert = new FraudAlert
            {
                UserId = userId,
                AlertTime = DateTime.UtcNow,
                AlertType = "Login",
                AlertDetails = string.Join(", ", suspiciousFactors),
                Resolved = false,
                Resolution = "Pending" // Add a default value for Resolution
            };

            _context.FraudAlerts.Add(alert);
            await _context.SaveChangesAsync();
        }

        // Add these methods to the FraudDetectionService class
        public async Task<FraudAlert> GetAlertById(int alertId)
        {
            return await _context.FraudAlerts.FindAsync(alertId);
        }

        public async Task ResolveAlert(int alertId, string resolution)
        {
            var alert = await _context.FraudAlerts.FindAsync(alertId);

            if (alert != null)
            {
                alert.Resolved = true;
                alert.ResolvedTime = DateTime.UtcNow;
                alert.Resolution = resolution;

                await _context.SaveChangesAsync();
            }
        }
    }
}