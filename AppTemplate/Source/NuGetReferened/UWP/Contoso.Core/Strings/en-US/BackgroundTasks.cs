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
// 	Generated: 03/25/2017 09:42:09
// </auto-generatedInfo>
// --------------------------------------------------------------------------------------------------
namespace Contoso.Core.Strings
{
    using Windows.ApplicationModel.Resources;
    
    
    public partial class BackgroundTasks
    {
        
        private static ResourceLoader resourceLoader;
        
        static BackgroundTasks()
        {
            try
            {
                string executingAssemblyName;
                executingAssemblyName = Windows.UI.Xaml.Application.Current.GetType().AssemblyQualifiedName;
                string[] executingAssemblySplit;
                executingAssemblySplit = executingAssemblyName.Split(',');
                executingAssemblyName = executingAssemblySplit[1];
                string currentAssemblyName;
                currentAssemblyName = typeof(BackgroundTasks).AssemblyQualifiedName;
                string[] currentAssemblySplit;
                currentAssemblySplit = currentAssemblyName.Split(',');
                currentAssemblyName = currentAssemblySplit[1];
                if (executingAssemblyName.Equals(currentAssemblyName))
                {
                    resourceLoader = ResourceLoader.GetForCurrentView("BackgroundTasks");
                }
                else
                {
                    resourceLoader = ResourceLoader.GetForCurrentView(currentAssemblyName + "/BackgroundTasks");
                }
            }
            catch
            {
                resourceLoader = ResourceLoader.GetForViewIndependentUse(typeof(BackgroundTasks).AssemblyQualifiedName.Split(',')[1].Trim() + "/" + nameof(BackgroundTasks));
            }
        }
        
        /// <summary>
        /// Localized resource similar to "Background tasks have not been enabled for this app. Use the manage button and ensure this app has been enabled to run in the background."
        /// </summary>
        public static string TextBackgroundAppDisabledStatus
        {
            get
            {
                return resourceLoader.GetString("TextBackgroundAppDisabledStatus");
            }
        }
    }
}
