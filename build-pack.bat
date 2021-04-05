@echo off
		if exist "c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
	        	echo will use VS 2019 Community build tools
        		set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"
		) else ( 
			if exist "c:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
        			set msbuild="c:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin"
	        		echo will use VS 2019 Pro build tools
			) else (
				echo Unable to locate VS 2017 or 2019 build tools, will use default build tools 
			)
		
)

set cwd=%cd%
set nuget="%cwd%\.nuget\nuget.exe"
echo Will use NUGET in %nuget%
echo Will use MSBUILD in %msbuild%

if [%1] == [] (
	%msbuild%\msbuild.exe santeguard.sln /t:clean /t:restore /t:rebuild /p:configuration=debug /m
) else (
	%msbuild%\msbuild.exe santeguard.sln /t:clean /t:restore /p:VersionNumber=%1 /m
	%msbuild%\msbuild.exe santeguard.sln /t:rebuild /p:configuration=debug /p:VersionNumber=%1 /m
)

FOR /R "%cwd%" %%G IN (*.nuspec) DO (
	echo Packing %%~pG
	pushd %%~pG
	%nuget% pack -OutputDirectory "%localappdata%\NugetStaging" -prop Configuration=Debug -prop VersionNumber=%1 -symbols
	popd
)