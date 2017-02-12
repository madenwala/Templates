using Newtonsoft.Json;

namespace Contoso.Api.Models
{
    public class Coordinates
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Validates latitude and longitude values.
        /// </summary>
        /// <param name="latitude">Latitude value to test.</param>
        /// <param name="longitude">Longitude value to test.</param>
        /// <returns>True if valid, else false.</returns>
        public static bool Validate(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                return false;
            else if (longitude < -180 || latitude > 180)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Validates latitude and longitude values.
        /// </summary>
        /// <param name="coordinates">Coordinate object to test.</param>
        /// <returns>True if valid, else false.</returns>
        public static bool Validate(Coordinates coordinates)
        {
            if (coordinates == null)
                return false;
            else
                return Validate(coordinates.Latitude, coordinates.Longitude);
        }
    }
}
