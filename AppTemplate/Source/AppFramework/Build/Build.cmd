msbuild ..\..\AppFramework\AppFramework\AppFramework.csproj /p:Configuration="Debug" /p:Platform="AnyCPU"

msbuild ..\..\AppFramework\AppFramework.Uwp\AppFramework.Uwp.csproj /p:Configuration="Debug" /p:Platform="x64"
msbuild ..\..\AppFramework\AppFramework.Uwp\AppFramework.Uwp.csproj /p:Configuration="Debug" /p:Platform="x86"
msbuild ..\..\AppFramework\AppFramework.Uwp\AppFramework.Uwp.csproj /p:Configuration="Debug" /p:Platform="arm"

xcopy ..\..\AppFramework\AppFramework.Uwp\bin\x86\Debug\* Temp\* /Y
corflags /32BITREQ- Temp\AppFramework.Uwp.dll

Build-NuGet-Package.cmd