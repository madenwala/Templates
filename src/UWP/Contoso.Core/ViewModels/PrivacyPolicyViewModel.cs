using Contoso.Core.Strings;

namespace Contoso.Core.ViewModels
{
    public sealed class PrivacyPolicyViewModel : WebViewModel
    {
        public PrivacyPolicyViewModel(bool showNavigation = true) : base(new AppFramework.Core.Models.WebViewArguments("http://go.microsoft.com/fwlink/?LinkId=521839", false, Resources.ViewTitlePrivacyPolicy))
        {
        }
    }
}