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
// 	Generated: 03/17/2017 06:12:38
// </auto-generatedInfo>
// --------------------------------------------------------------------------------------------------
namespace AppFramework.Core.Strings
{
    using Windows.ApplicationModel.Resources;
    
    
    public partial class WebBrowser
    {
        
        private static ResourceLoader resourceLoader;
        
        static WebBrowser()
        {
            string executingAssemblyName;
            executingAssemblyName = Windows.UI.Xaml.Application.Current.GetType().AssemblyQualifiedName;
            string[] executingAssemblySplit;
            executingAssemblySplit = executingAssemblyName.Split(',');
            executingAssemblyName = executingAssemblySplit[1];
            string currentAssemblyName;
            currentAssemblyName = typeof(WebBrowser).AssemblyQualifiedName;
            string[] currentAssemblySplit;
            currentAssemblySplit = currentAssemblyName.Split(',');
            currentAssemblyName = currentAssemblySplit[1];
            if (executingAssemblyName.Equals(currentAssemblyName))
            {
                resourceLoader = ResourceLoader.GetForCurrentView("WebBrowser");
            }
            else
            {
                resourceLoader = ResourceLoader.GetForCurrentView(currentAssemblyName + "/WebBrowser");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Go back"
        /// </summary>
        public static string TextGoBack
        {
            get
            {
                return resourceLoader.GetString("TextGoBack");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Go forward"
        /// </summary>
        public static string TextGoForward
        {
            get
            {
                return resourceLoader.GetString("TextGoForward");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Web Browser"
        /// </summary>
        public static string TextWebDefaultTitle
        {
            get
            {
                return resourceLoader.GetString("TextWebDefaultTitle");
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Could not load the requested page. Try again later."
        /// </summary>
        public static string TextWebErrorGeneric
        {
            get
            {
                return resourceLoader.GetString("TextWebErrorGeneric");
            }
        }
    }
}
