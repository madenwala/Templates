using Contoso.Core.Strings;

namespace Contoso.Core.ViewModels
{
    public sealed class TermsOfServiceViewModel : WebViewModel
    {
        #region Constructors

        public TermsOfServiceViewModel() : base(new AppFramework.Core.Models.WebViewArguments("http://go.microsoft.com/fwlink/?LinkID=206977", false, Resources.ViewTitleTermsOfService))
        {
        }

        #endregion
    }
}