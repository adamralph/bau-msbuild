@ECHO OFF

scriptcs -install
scriptcs baufile.csx

ECHO [Dog] WARNING: If anything goes wrong here, you may end up with an altered packages.config. If so, revert the changes before doing anything.

ECHO [Dog] Removing Bau.MSBuild package folder...
rmdir packages\Bau.MSBuild.0.1.0-adhoc /S /Q

ECHO [Dog] Copying Bau.MSBuild from bin\Release...
xcopy src\Bau.MSBuild\bin\Release\Bau.MSBuild.dll packages\Bau.MSBuild.0.1.0-adhoc\lib\net45\ /Y /Q

ECHO [Dog] Copying Bau.MSBuild NuGet package from artifacts\output...
xcopy artifacts\output\Bau.MSBuild.0.1.0-adhoc.nupkg packages\Bau.MSBuild.0.1.0-adhoc /Y /Q

ECHO [Dog] Taking copy of original packages.config...
copy packages.config packages.orig.config /Y

ECHO [Dog] Overwriting packages.config with dog.config...
copy dog.config packages.config /Y

ECHO [Dog] Running dog.csx...
scriptcs dog.csx -- %*

ECHO [Dog] Overwriting modified packages.config with original packages.config...
copy packages.orig.config packages.config /Y

ECHO [Dog] Deleting copy of original packages.config...
del packages.orig.config
