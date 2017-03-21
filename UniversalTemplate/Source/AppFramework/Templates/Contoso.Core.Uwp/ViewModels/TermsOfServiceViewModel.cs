using Contoso.Core.Strings;
using AppFramework.Core.ViewModels;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public partial class TermsOfServiceViewModel : WebViewModel
    {
        #region Properties

        /// <summary>
        /// Gets the title to be displayed on the view consuming this ViewModel.
        /// </summary>
        public override string Title
        {
            get { return Resources.ViewTitleTermsOfService; }
        }

        #endregion

        #region Constructors

        public TermsOfServiceViewModel(bool showNavigation = true) : base(showNavigation)
        {
            if (DesignMode.DesignModeEnabled)
                return;
        }

        #endregion

        #region Methods

        public override void InitialNavigation()
        {
            this.NavigateTo("http://go.microsoft.com/fwlink/?LinkID=206977");
        }

        #endregion
    }
}