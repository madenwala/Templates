//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// --------------------------------------------------------------------------------------------------
// <auto-generatedInfo>
// 	This code was generated by ResW File Code Generator (http://reswcodegen.codeplex.com)
// 	ResW File Code Generator was written by Christian Resma Helle
// 	and is under GNU General Public License version 2 (GPLv2)
// 
// 	This code contains a helper class exposing property representations
// 	of the string resources defined in the specified .ResW file
// 
// 	Generated: 04/25/2017 00:21:51
// </auto-generatedInfo>
// --------------------------------------------------------------------------------------------------
namespace AppFramework.Core.Strings
{
    using Windows.ApplicationModel.Resources;
    
    
    public partial class Resources
    {
        
        private static ResourceLoader resourceLoader;
        
        static Resources()
        {
            try
            {
                string executingAssemblyName;
                executingAssemblyName = Windows.UI.Xaml.Application.Current.GetType().AssemblyQualifiedName;
                string[] executingAssemblySplit;
                executingAssemblySplit = executingAssemblyName.Split(',');
                executingAssemblyName = executingAssemblySplit[1];
                string currentAssemblyName;
                currentAssemblyName = typeof(Resources).AssemblyQualifiedName;
                string[] currentAssemblySplit;
                currentAssemblySplit = currentAssemblyName.Split(',');
                currentAssemblyName = currentAssemblySplit[1];
                if (executingAssemblyName.Equals(currentAssemblyName))
                {
                    resourceLoader = ResourceLoader.GetForCurrentView("Resources");
                }
                else
                {
                    resourceLoader = ResourceLoader.GetForCurrentView(currentAssemblyName + "/Resources");
                }
            }
            catch (System.Exception )
            {
                resourceLoader = ResourceLoader.GetForViewIndependentUse(typeof(Resources).AssemblyQualifiedName.Split(',')[1].Trim() + "/Resources");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Please describe what you were doing when the problem occurred:
        ///--------------------
        ///
        ///
        ///--------------------"
        /// </summary>
        public static string ApplicationProblemEmailBodyTemplate
        {
            get
            {
                return resourceLoader.GetString("ApplicationProblemEmailBodyTemplate");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "{0} {1} auto-generated problem report"
        /// </summary>
        public static string ApplicationProblemEmailSubjectTemplate
        {
            get
            {
                return resourceLoader.GetString("ApplicationProblemEmailSubjectTemplate");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "A problem occurred the last time you ran this application. Would you like to send us an email to report it?"
        /// </summary>
        public static string ApplicationProblemPromptMessage
        {
            get
            {
                return resourceLoader.GetString("ApplicationProblemPromptMessage");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Problem Report"
        /// </summary>
        public static string ApplicationProblemPromptTitle
        {
            get
            {
                return resourceLoader.GetString("ApplicationProblemPromptTitle");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Check out {0} from the Windows Store!
        ///
        ///{1}"
        /// </summary>
        public static string ApplicationSharingBodyText
        {
            get
            {
                return resourceLoader.GetString("ApplicationSharingBodyText");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Please provide and feedback you'd like to share with support:
        ///--------------------
        ///
        ///
        ///--------------------
        ///
        ///"
        /// </summary>
        public static string ApplicationSupportEmailBodyTemplate
        {
            get
            {
                return resourceLoader.GetString("ApplicationSupportEmailBodyTemplate");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Feedback for {0} {1}"
        /// </summary>
        public static string ApplicationSupportEmailSubjectTemplate
        {
            get
            {
                return resourceLoader.GetString("ApplicationSupportEmailSubjectTemplate");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Clear Cache"
        /// </summary>
        public static string ClearAppCacheText
        {
            get
            {
                return resourceLoader.GetString("ClearAppCacheText");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Would you like to send an e-mail with feedback instead?"
        /// </summary>
        public static string PromptRateApplicationEmailFeedbackMessage
        {
            get
            {
                return resourceLoader.GetString("PromptRateApplicationEmailFeedbackMessage");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Share feedback"
        /// </summary>
        public static string PromptRateApplicationEmailFeedbackTitle
        {
            get
            {
                return resourceLoader.GetString("PromptRateApplicationEmailFeedbackTitle");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Would you like to rate this application and provide feedback on how we can make things better?"
        /// </summary>
        public static string PromptRateApplicationMessage
        {
            get
            {
                return resourceLoader.GetString("PromptRateApplicationMessage");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Rate Application"
        /// </summary>
        public static string PromptRateApplicationTitle
        {
            get
            {
                return resourceLoader.GetString("PromptRateApplicationTitle");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Cancel"
        /// </summary>
        public static string TextCancel
        {
            get
            {
                return resourceLoader.GetString("TextCancel");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Canceled..."
        /// </summary>
        public static string TextCancellationRequested
        {
            get
            {
                return resourceLoader.GetString("TextCancellationRequested");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "An unexpected error has occured. We apologize for any inconveniences here and we will be working to resolve this soon."
        /// </summary>
        public static string TextErrorGeneric
        {
            get
            {
                return resourceLoader.GetString("TextErrorGeneric");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "No items to display."
        /// </summary>
        public static string TextListNoData
        {
            get
            {
                return resourceLoader.GetString("TextListNoData");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Loading..."
        /// </summary>
        public static string TextLoading
        {
            get
            {
                return resourceLoader.GetString("TextLoading");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Not right now"
        /// </summary>
        public static string TextMaybeLater
        {
            get
            {
                return resourceLoader.GetString("TextMaybeLater");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "No"
        /// </summary>
        public static string TextNo
        {
            get
            {
                return resourceLoader.GetString("TextNo");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "No internet connection available. Connect to internet and try again."
        /// </summary>
        public static string TextNoInternet
        {
            get
            {
                return resourceLoader.GetString("TextNoInternet");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "N/A"
        /// </summary>
        public static string TextNotApplicable
        {
            get
            {
                return resourceLoader.GetString("TextNotApplicable");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "OK"
        /// </summary>
        public static string TextOk
        {
            get
            {
                return resourceLoader.GetString("TextOk");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Yes"
        /// </summary>
        public static string TextYes
        {
            get
            {
                return resourceLoader.GetString("TextYes");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Privacy Policy"
        /// </summary>
        public static string ViewTitlePrivacyPolicy
        {
            get
            {
                return resourceLoader.GetString("ViewTitlePrivacyPolicy");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Terms of Service"
        /// </summary>
        public static string ViewTitleTermsOfService
        {
            get
            {
                return resourceLoader.GetString("ViewTitleTermsOfService");
            }
        }
    }
}
