msbuild ..\AppFramework.Core\AppFramework.Core.csproj /p:Configuration="Release"
msbuild ..\AppFramework.Core.Uwp\AppFramework.Core.Uwp.csproj /p:Configuration="Release" /p:Platform="x64"
msbuild ..\AppFramework.UI.Uwp\AppFramework.UI.Uwp.csproj /p:Configuration="Release" /p:Platform="x64"