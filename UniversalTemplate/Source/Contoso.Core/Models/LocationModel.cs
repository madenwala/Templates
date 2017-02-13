using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Contoso.Core.Models
{
    /// <summary>
    /// Interface that any model containing latitude/longitude should implement.
    /// </summary>
    public interface ILocationModel : IModel
    {
        double Latitude { get; set; }

        double Longitude { get; set; }

        string LocationDisplayName { get; set; }

        void SetDistanceAway(ILocationModel loc);
    }

    /// <summary>
    /// Models that contain latitude/longitude can inherit from this class to gain ModelBase + ILocationModel functionality and
    /// any other functionality common to location based models.
    /// </summary>
    public class LocationModel : ModelBase, ILocationModel
    {
        #region Constants

        public const int CoordinateDecimalPlaces = 6;
        public const int DistanceDecimalPlaces = 2;

        #endregion Constants

        #region Constructors

        static LocationModel()
        {
            var cultureName = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("longdate", new[] { "US" }).ResolvedLanguage;
            IsMetric = new System.Globalization.RegionInfo(cultureName).IsMetric;
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
                if (this.Latitude == double.NaN || this.Longitude == double.NaN)
                    return Strings.Resources.TextNotApplicable;
                if (string.IsNullOrWhiteSpace(_displayName))
                    return string.Format("{0}, {1}", this.Latitude, this.Longitude);
                else
                    return _displayName;
            }
            set
            {
                if (value != _displayName)
                {
                    _displayName = value;
                    this.NotifyPropertyChanged(() => this.LocationDisplayName);
                }
            }
        }

        private double _Latitude = double.MinValue;
        /// <summary>
        /// Gets or sets the latitude of this object.
        /// </summary>
        [JsonProperty("lat")]
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
        [JsonProperty("lon")]
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
            set
            {
                if (this.SetProperty(ref _DistanceAway, value))
                    this.NotifyPropertyChanged(() => this.DistanceAwayDisplay);
            }
        }

        /// <summary>
        /// Gets a friendly display for the distance away.
        /// </summary>
        public string DistanceAwayDisplay
        {
            get
            {
                return string.Format(Strings.Location.TextDistanceAwayDisplay, this.DistanceAway.ToString("N"), IsMetric ? Strings.Location.TextKilomentersShort : Strings.Location.TextMilesShort);
            }
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
        }

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

        #endregion Methods
    }

    /// <summary>
    /// Collection for holding LocationModel instance.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class LocationModelList<TItem> : ModelList<TItem> where TItem : ILocationModel
    {
        #region Constructors

        public LocationModelList() : base()
        {
        }

        public LocationModelList(IEnumerable<TItem> items) : base(items)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the distance away for all location model instances in this collection.
        /// </summary>
        /// <param name="loc"></param>
        public void SetDistancesAway(ILocationModel loc)
        {
            foreach (var item in this)
                item.SetDistanceAway(loc);
        }

        #endregion
    }

    public interface IUniqueLocationModel : IUniqueModel, ILocationModel
    {
    }

    public class UniqueLocationModel<T> : LocationModel, IUniqueLocationModel, IEquatable<IUniqueModel>
    {
        #region Properties

        private string _ID;
        public virtual string ID
        {
            get { return _ID; }
            set { this.SetProperty(ref _ID, value); }
        }

        #endregion

        #region Methods

        #region Equatable Functions

        public bool Equals(IUniqueModel other)
        {
            return other != null && this.ID?.Equals(other.ID) == true;
        }

        public override bool Equals(object obj)
        {
            if (obj is IUniqueModel)
                return this.Equals((IUniqueModel)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public static bool operator ==(UniqueLocationModel<T> obj1, IUniqueModel obj2)
        {
            return (object)obj1 == null || (object)obj2 == null ? Equals(obj1, obj2) : obj1.Equals(obj2);
        }

        public static bool operator !=(UniqueLocationModel<T> obj1, IUniqueModel obj2)
        {
            return obj1 == null || obj2 == null ? !Equals(obj1, obj2) : !obj1.Equals(obj2);
        }

        #endregion

        #endregion
    }

    public class UniqueLocationModelList<TItem, TIdType> : UniqueModelList<TItem> where TItem : IUniqueLocationModel
    {
        #region Constructors

        public UniqueLocationModelList()
        {
        }

        public UniqueLocationModelList(IEnumerable<TItem> items)
        {
            this.AddRange(items);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the distance away for all location model instances in this collection.
        /// </summary>
        /// <param name="loc"></param>
        public void SetDistancesAway(ILocationModel loc)
        {
            foreach (var item in this)
                item.SetDistanceAway(loc);
        }

        #endregion
    }
}