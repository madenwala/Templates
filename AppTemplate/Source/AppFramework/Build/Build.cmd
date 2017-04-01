msbuild ..\..\AppFramework\AppFramework.Core\AppFramework.Core.csproj /p:Configuration="Debug"

msbuild ..\..\AppFramework\AppFramework.Core.Uwp\AppFramework.Core.Uwp.csproj /p:Configuration="Debug" /p:Platform="x64"
msbuild ..\..\AppFramework\AppFramework.UI.Uwp\AppFramework.UI.Uwp.csproj /p:Configuration="Debug" /p:Platform="x64"

msbuild ..\..\AppFramework\AppFramework.Core.Uwp\AppFramework.Core.Uwp.csproj /p:Configuration="Debug" /p:Platform="x86"
msbuild ..\..\AppFramework\AppFramework.UI.Uwp\AppFramework.UI.Uwp.csproj /p:Configuration="Debug" /p:Platform="x86"

msbuild ..\..\AppFramework\AppFramework.Core.Uwp\AppFramework.Core.Uwp.csproj /p:Configuration="Debug" /p:Platform="arm"
msbuild ..\..\AppFramework\AppFramework.UI.Uwp\AppFramework.UI.Uwp.csproj /p:Configuration="Debug" /p:Platform="arm"

xcopy ..\..\AppFramework\AppFramework.Core.Uwp\bin\x86\Debug\* Temp\* /Y
corflags /32BITREQ- Temp\AppFramework.Core.Uwp.dll
corflags /32BITREQ- Temp\AppFramework.UI.Uwp.dll

Build-NuGet-Package.cmd