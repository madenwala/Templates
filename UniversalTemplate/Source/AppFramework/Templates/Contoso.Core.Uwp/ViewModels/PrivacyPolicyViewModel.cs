using Contoso.Core.Strings;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public partial class PrivacyPolicyViewModel : WebViewModel
    {
        #region Properties

        /// <summary>
        /// Gets the title to be displayed on the view consuming this ViewModel.
        /// </summary>
        public override string Title
        {
            get { return Resources.ViewTitlePrivacyPolicy; }
        }

        #endregion Properties

        #region Constructors

        public PrivacyPolicyViewModel(bool showNavigation = true) : base(showNavigation)
        {
            if (DesignMode.DesignModeEnabled)
                return;
        }

        #endregion Constructors

        #region Methods

        public override void InitialNavigation()
        {
            this.NavigateTo("http://go.microsoft.com/fwlink/?LinkId=521839");
        }

        #endregion Methods
    }
}