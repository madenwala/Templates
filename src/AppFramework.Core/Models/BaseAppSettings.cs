namespace AppFramework.Core.Models
{
    /// <summary>
    /// Container class for local application settings.  Create all your local app setting properties here.
    /// </summary>
    public abstract class BaseAppSettingsLocal : BaseModel
    {
        #region Properties

        private BaseLocationModel _LocationLastKnown;
        /// <summary>
        /// Gets or sets the last known location for the user.
        /// </summary>
        public BaseLocationModel LocationLastKnown
        {
            get { return _LocationLastKnown; }
            set { this.SetProperty(ref _LocationLastKnown, value); }
        }

        #endregion
    }

    /// <summary>
    /// Container class for roaming application settings.  Create all your roaming app setting properties here.
    /// </summary>
    public abstract class BaseAppSettingsRoaming : BaseModel
    {
        #region Properties

        private int _ApplicationTheme = 0;
        /// <summary>
        /// Gets or sets the application theme desired by the user.
        /// </summary>
        public int ApplicationTheme
        {
            get { return _ApplicationTheme; }
            set { this.SetProperty(ref _ApplicationTheme, value); }
        }

        private bool _EnableFullLogging = false;
        /// <summary>
        /// Gets or sets whether or not debug logging is enabled.
        /// </summary>
        public bool EnableFullLogging
        {
            get { return _EnableFullLogging; }
            set { this.SetProperty(ref _EnableFullLogging, value); }
        }

        #endregion
    }
}