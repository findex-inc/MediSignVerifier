cls 
prompt $g

rem ----------------------------------------------------
rem Create ThirdPartyNotices.txt with nuget-license
rem (https://github.com/sensslen/nuget-license)
rem ----------------------------------------------------
rem [install tool]
rem dotnet tool install --global nuget-license
rem [uninstall tool]
rem dotnet tool uninstall --global nuget-license


rem MediSignVerifier library

set libdir=..\src\MediSignVerifier
nuget-license -i %libdir%\MediSignVerifier.csproj -override %libdir%\ThirdPartyOverride.json > %libdir%\ThirdPartyNotices.txt


rem EPDVerifyCmd application

set epcmddir=..\src\EPDVerifyCmd
nuget-license -i %epcmddir%\EPDVerifyCmd.csproj -override %epcmddir%\ThirdPartyOverride.json > %epcmddir%\ThirdPartyNotices.txt

pause
