msbuild ..\..\AppFramework\AppFramework.Uwp\AppFramework.Uwp.csproj /p:Configuration="Release" /p:Platform="x64"
msbuild ..\..\AppFramework\AppFramework.Uwp\AppFramework.Uwp.csproj /p:Configuration="Release" /p:Platform="x86"
msbuild ..\..\AppFramework\AppFramework.Uwp\AppFramework.Uwp.csproj /p:Configuration="Release" /p:Platform="arm"

xcopy ..\..\AppFramework\AppFramework.Uwp\bin\x86\Release\* Temp\* /Y
corflags /32BITREQ- Temp\AppFramework.Uwp.dll
corflags /32BITREQ- Temp\AppFramework.NetStandard.dll

Build-NuGet-Package.cmd