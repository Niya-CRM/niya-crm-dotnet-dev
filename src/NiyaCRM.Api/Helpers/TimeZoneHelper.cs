using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using NodaTime.TimeZones;

namespace NiyaCRM.Api.Helpers
{
    /// <summary>
    /// Helper class for time zone operations using NodaTime
    /// </summary>
    public static class TimeZoneHelper
    {
        private static readonly IDateTimeZoneProvider _tzProvider = DateTimeZoneProviders.Tzdb;
        
        /// <summary>
        /// Gets a dictionary of common IANA time zones with user-friendly display names
        /// </summary>
        /// <returns>Dictionary with IANA time zone ID as key and friendly name as value</returns>
        public static Dictionary<string, string> GetCommonIanaTimeZones()
        {
            return new Dictionary<string, string>
            {
                { "UTC", "UTC (Coordinated Universal Time)" },
                
                // Americas
                { "America/New_York", "Eastern Time (US & Canada)" },
                { "America/Chicago", "Central Time (US & Canada)" },
                { "America/Denver", "Mountain Time (US & Canada)" },
                { "America/Los_Angeles", "Pacific Time (US & Canada)" },
                { "America/Anchorage", "Alaska" },
                { "America/Halifax", "Atlantic Time (Canada)" },
                { "America/St_Johns", "Newfoundland" },
                { "America/Sao_Paulo", "Brasilia" },
                { "America/Mexico_City", "Mexico City" },
                { "America/Buenos_Aires", "Buenos Aires" },
                { "America/Bogota", "Bogota, Lima" },
                { "America/Caracas", "Caracas" },
                
                // Europe/Africa
                { "Europe/London", "London, Dublin, Edinburgh" },
                { "Europe/Berlin", "Berlin, Stockholm, Rome, Vienna" },
                { "Europe/Paris", "Paris, Madrid, Brussels" },
                { "Europe/Athens", "Athens, Istanbul" },
                { "Europe/Moscow", "Moscow, St. Petersburg" },
                { "Europe/Helsinki", "Helsinki" },
                { "Africa/Cairo", "Cairo" },
                { "Africa/Johannesburg", "Johannesburg" },
                { "Africa/Nairobi", "Nairobi" },
                
                // Asia/Middle East
                { "Asia/Dubai", "Dubai, Abu Dhabi" },
                { "Asia/Kolkata", "Mumbai, New Delhi" },
                { "Asia/Karachi", "Karachi, Islamabad" },
                { "Asia/Bangkok", "Bangkok, Hanoi, Jakarta" },
                { "Asia/Shanghai", "Beijing, Shanghai" },
                { "Asia/Singapore", "Singapore, Kuala Lumpur" },
                { "Asia/Tokyo", "Tokyo, Seoul" },
                { "Asia/Hong_Kong", "Hong Kong" },
                { "Asia/Taipei", "Taipei" },
                
                // Pacific
                { "Pacific/Honolulu", "Hawaii" },
                { "Australia/Sydney", "Sydney, Melbourne" },
                { "Australia/Perth", "Perth" },
                { "Australia/Brisbane", "Brisbane" },
                { "Pacific/Auckland", "Auckland, Wellington" },
                { "Pacific/Fiji", "Fiji" }
            };
        }

        /// <summary>
        /// Converts a Windows time zone ID to an IANA time zone ID
        /// </summary>
        /// <param name="timeZoneId">Time zone ID (Windows or IANA)</param>
        /// <returns>IANA time zone ID or UTC if conversion fails</returns>
        public static string ConvertWindowsToIana(string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                return "UTC";
            }

            // If it's already an IANA ID (contains a slash), return as is
            if (timeZoneId.Contains('/'))
            {
                return timeZoneId;
            }

            try
            {
                // Try to map Windows time zone to IANA using mapping from NodaTime
                var mappings = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
                
                // Find matching Windows ID directly with FirstOrDefault
                var matchingMapping = mappings
                    .FirstOrDefault(mapping => string.Equals(mapping.WindowsId, timeZoneId, StringComparison.OrdinalIgnoreCase));
                
                if (matchingMapping != null)
                {
                    // Return the first territory mapping (usually "001" which is the default)
                    return matchingMapping.TzdbIds.FirstOrDefault() ?? "UTC";
                }
                
                // If no mapping found, try to find a close match
                var systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                var baseUtcOffset = systemTimeZone.BaseUtcOffset;
                
                // Find IANA zones with similar offset
                var availableZones = _tzProvider.Ids
                    .Select(id => _tzProvider[id])
                    .Where(tz => Math.Abs((tz.GetUtcOffset(Instant.FromDateTimeUtc(DateTime.UtcNow)).Ticks / TimeSpan.TicksPerHour) - 
                                  baseUtcOffset.TotalHours) < 0.1)
                    .Select(tz => tz.Id)
                    .FirstOrDefault();
                
                return availableZones ?? "UTC";
            }
            catch
            {
                // Return UTC as fallback if conversion fails
                return "UTC";
            }
        }
        
        /// <summary>
        /// Gets all IANA time zones available from NodaTime's TZDB database
        /// </summary>
        /// <returns>Dictionary with IANA time zone ID as key and display name as value</returns>
        public static Dictionary<string, string> GetAllIanaTimeZones()
        {
            var result = new Dictionary<string, string>();
            var now = SystemClock.Instance.GetCurrentInstant();
            
            // Add UTC first
            result.Add("UTC", "(GMT+00:00) Coordinated Universal Time (UTC)");
            
            // Get all IANA time zones from NodaTime
            foreach (var tzId in _tzProvider.Ids.OrderBy(id => id))
            {
                // Skip Etc/* timezones which are just generic GMT+X or GMT-X timezones
                if (tzId.StartsWith("Etc/") && tzId != "Etc/UTC")
                    continue;
                    
                // Skip other problematic or redundant timezones
                if (ShouldSkipTimeZone(tzId))
                    continue;
                    
                if (!result.ContainsKey(tzId))
                {
                    try
                    {
                        var tz = _tzProvider[tzId];
                        var offset = tz.GetUtcOffset(now);
                        var offsetStr = offset.ToString("+HH:mm", null);
                        
                        // Format the display name with offset and location
                        var displayName = FormatTimeZoneDisplayName(tzId, offsetStr);
                        result.Add(tzId, displayName);
                    }
                    catch
                    {
                        // Skip time zones that can't be loaded
                    }
                }
            }
            
            return result.OrderBy(tz => tz.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        
        /// <summary>
        /// Formats a time zone ID into a user-friendly display name
        /// </summary>
        /// <param name="tzId">IANA time zone ID</param>
        /// <param name="offsetStr">UTC offset string</param>
        /// <returns>Formatted display name</returns>
        private static string FormatTimeZoneDisplayName(string tzId, string offsetStr)
        {
            // Extract location name from IANA ID
            var parts = tzId.Split('/');
            var location = parts.Length > 1 ? parts[parts.Length - 1] : tzId;
            
            // Replace underscores with spaces
            location = location.Replace("_", " ");
            
            // Get a friendly time zone name based on location
            var friendlyName = GetFriendlyTimeZoneName(location, tzId);
            
            // Format display name with GMT offset, friendly name, and IANA ID
            return $"(GMT{offsetStr}) {friendlyName} ({tzId})";
        }
        
        /// <summary>
        /// Gets a friendly name for a time zone based on its location
        /// </summary>
        /// <param name="location">Location extracted from IANA ID</param>
        /// <param name="tzId">Full IANA time zone ID</param>
        /// <returns>Friendly time zone name</returns>
        private static string GetFriendlyTimeZoneName(string location, string tzId)
        {
            // Map common locations to more user-friendly names
            return tzId switch
            {
                "Pacific/Auckland" => "New Zealand Standard Time",
                "America/New_York" => "Eastern Standard Time",
                "America/Chicago" => "Central Standard Time",
                "America/Denver" => "Mountain Standard Time",
                "America/Los_Angeles" => "Pacific Standard Time",
                "Europe/London" => "GMT Standard Time",
                "Europe/Berlin" => "Central European Standard Time",
                "Europe/Paris" => "Central European Standard Time",
                "Asia/Tokyo" => "Tokyo Standard Time",
                "Australia/Sydney" => "AUS Eastern Standard Time",
                "Asia/Kolkata" => "India Standard Time",
                "Asia/Shanghai" => "China Standard Time",
                "Asia/Dubai" => "Arabian Standard Time",
                "Asia/Singapore" => "Singapore Standard Time",
                "Asia/Hong_Kong" => "Hong Kong Standard Time",
                _ => $"{char.ToUpper(location[0])}{location.Substring(1)} Standard Time",// For other locations, create a name based on the location
            };

        }
        
        /// <summary>
        /// Determines if a timezone should be skipped from the list
        /// </summary>
        /// <param name="tzId">IANA timezone ID</param>
        /// <returns>True if the timezone should be skipped</returns>
        private static bool ShouldSkipTimeZone(string tzId)
        {
            // List of prefixes to skip (duplicates or problematic timezones)
            var prefixesToSkip = new[] {
                "GMT", "SystemV/", "US/", "Canada/", "Brazil/", "Mexico/", "Chile/", 
                "Cuba", "Egypt", "GB", "Hongkong", "Iceland", "Iran", "Israel", "Jamaica", 
                "Japan", "Kwajalein", "Libya", "Navajo", "Poland", "Portugal", "Singapore", 
                "Turkey", "Factory", "Universal", "UCT", "ROC", "ROK", "W-SU", "NZ", 
                "PRC", "MST", "HST", "EST", "CST6", "Eire"
            };
            
            // List of exact timezone IDs to skip
            var exactIdsToSkip = new[] {
                "PST8PDT", "MST7MDT", "CST6CDT", "EST5EDT", "EET", "CET", "MET", "WET"
            };
            
            // Check if the timezone ID starts with any of the prefixes to skip
            return prefixesToSkip.Any(prefix => tzId.StartsWith(prefix)) ||
                   exactIdsToSkip.Any(id => tzId.Equals(id)) ||
                   tzId.Contains("Zulu"); // Special case for Zulu
        }
    }
}
