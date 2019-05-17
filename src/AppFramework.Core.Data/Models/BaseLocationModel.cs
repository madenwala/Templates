//using Newtonsoft.Json;
using System;

namespace AppFramework.Core.Models
{
    /// <summary>
    /// Models that contain latitude/longitude can inherit from this class to gain ModelBase + ILocationModel functionality and
    /// any other functionality common to location based models.
    /// </summary>
    public abstract class BaseLocationModel : BaseModel, ILocationModel
    {
        #region Constants

        public static int CoordinateDecimalPlaces = 6;
        public static int DistanceDecimalPlaces = 2;

        #endregion Constants

        #region Constructors

        static BaseLocationModel()
        {
            // TODO Determine if metric system
            //var cultureName = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("longdate", new[] { "US" }).ResolvedLanguage;
            //IsMetric = new System.Globalization.RegionInfo(cultureName).IsMetric;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether or not the system is metric or imperial.
        /// </summary>
        public static bool IsMetric { get; private set; }

        private string _displayName = null;
        /// <summary>
        /// Gets or sets a friendly display name to summaries the latitude/longitude or the name of this location if set.
        /// </summary>
        public virtual string LocationDisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_displayName))
                    return string.Format("{0}, {1}", this.Latitude, this.Longitude);
                else
                    return _displayName;
            }
            set
            {
                if (this.SetProperty(ref _displayName, value))
                    this.NotifyPropertyChanged(() => this.LocationDisplayName);
            }
        }

        private double _Latitude = double.MinValue;
        /// <summary>
        /// Gets or sets the latitude of this object.
        /// </summary>
        // TODO [JsonProperty("lat")]
        public double Latitude
        {
            get
            {
                return _Latitude;
            }
            set
            {
                value = Math.Round(value, CoordinateDecimalPlaces);
                if (this.SetProperty(ref _Latitude, value))
                    this.NotifyPropertyChanged(() => this.LocationDisplayName);
            }
        }

        private double _Longitude = double.NaN;
        /// <summary>
        /// Gets or sets the longitude of this object.
        /// </summary>
        // TODO [JsonProperty("lon")]
        public double Longitude
        {
            get
            {
                return _Longitude;
            }
            set
            {
                value = Math.Round(value, CoordinateDecimalPlaces);
                if (this.SetProperty(ref _Longitude, value))
                    this.NotifyPropertyChanged(() => this.LocationDisplayName);
            }
        }

        private double _DistanceAway = double.NaN;
        /// <summary>
        /// Gets the distance away this object is. Use SetDistanceAway to update this property.
        /// </summary>
        public double DistanceAway
        {
            get { return _DistanceAway; }
            private set { this.SetProperty(ref _DistanceAway, value); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the latitude/longitude of this object.
        /// </summary>
        /// <param name="loc">Location information to update this object with.</param>
        /// <returns></returns>
        public bool SetLocation(ILocationModel loc)
        {
            bool coordUpdated = false;

            double locLatitue = loc == null ? double.NaN : loc.Latitude;
            double locLongitude = loc == null ? double.NaN : loc.Longitude;

            if (this.Latitude != locLatitue)
            {
                this.Latitude = locLatitue;
                coordUpdated = true;
            }

            if (this.Longitude != locLongitude)
            {
                this.Longitude = locLongitude;
                coordUpdated = true;
            }

            if (coordUpdated)
                this.LocationDisplayName = null;

            return coordUpdated;
        }

        /// <summary>
        /// Sets the distance away property of this object calculated from the specified location object.
        /// </summary>
        /// <param name="loc">Location object to calculate distance away with.</param>
        public void SetDistanceAway(ILocationModel loc)
        {
            this.DistanceAway = this.GetDistanceTo(loc);
            this.NotifyPropertyChanged(() => this.DistanceAwayDisplay);
        }

        /// <summary>
        /// Gets a friendly display for the distance away.
        /// </summary>
        public virtual string DistanceAwayDisplay { get; set; }

        /// <summary>
        /// Gets the distance away from a specified location object.
        /// </summary>
        /// <param name="loc">Location object to calculate distance away with.</param>
        /// <returns>Distance amount.</returns>
        public double GetDistanceTo(ILocationModel loc)
        {
            if (loc != null)
            {
                var r = IsMetric ? 6371 : 3960;
                var dLat = ToRadian(this.Latitude - loc.Latitude);
                var dLon = ToRadian(this.Longitude - loc.Longitude);
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadian(loc.Latitude)) * Math.Cos(ToRadian(this.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
                var d = r * c;
                return Math.Round(d, DistanceDecimalPlaces);
            }
            else
                return double.NaN;
        }

        private double ToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

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
        /// <param name="loc">Location model object to validate.</param>
        /// <returns>True if valid, else false.</returns>
        public static bool Validate(ILocationModel loc)
        {
            if (loc == null)
                return false;
            else
                return Validate(loc.Latitude, loc.Longitude);
        }

        #endregion Methods
    }
}
