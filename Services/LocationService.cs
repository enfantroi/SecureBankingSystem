using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SecureBankingSystem.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecureBankingSystem.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public LocationService(ApplicationDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetLocationFromIP(string ipAddress)
        {
            // For demo purposes, we'll use a free IP geolocation API
            // In a production environment, you would use a more reliable paid service
            try
            {
                var response = await _httpClient.GetAsync($"https://ipapi.co/{ipAddress}/json/");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(content);

                    // Format: "City, Country"
                    return $"{data["city"]}, {data["country_name"]}";
                }
            }
            catch (Exception)
            {
                // Log the exception in a real application
            }

            // If the API call fails, return a fallback value
            return "Unknown Location";
        }

        public async Task<double> CalculateGeoDistance(string location1, string location2)
        {
            // For simplicity in this demo, we'll simulate distance calculation
            // In a real application, you would use proper geospatial calculations

            // If either location is unknown, return a large distance
            if (location1 == "Unknown Location" || location2 == "Unknown Location")
                return 1000; // Arbitrary large distance

            if (location1 == location2)
                return 0;

            // Simulate some distance calculation based on string comparison
            // In a real app, you would parse coordinates and use the Haversine formula
            Random rnd = new Random();
            return rnd.Next(50, 5000); // Return random distance between 50-5000 miles
        }

        public async Task<bool> IsLocationCommonForUser(string userId, string location)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.CommonLocations))
                return false;

            // CommonLocations is stored as a JSON array of location strings
            try
            {
                var commonLocations = JsonSerializer.Deserialize<List<string>>(user.CommonLocations);
                return commonLocations.Contains(location);
            }
            catch
            {
                return false;
            }
        }

        public async Task AddCommonLocationForUser(string userId, string location)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return;

            List<string> commonLocations;

            if (string.IsNullOrEmpty(user.CommonLocations))
                commonLocations = new List<string>();
            else
            {
                try
                {
                    commonLocations = JsonSerializer.Deserialize<List<string>>(user.CommonLocations);
                }
                catch
                {
                    commonLocations = new List<string>();
                }
            }

            if (!commonLocations.Contains(location))
            {
                commonLocations.Add(location);
                user.CommonLocations = JsonSerializer.Serialize(commonLocations);
                await _context.SaveChangesAsync();
            }
        }
    }
}