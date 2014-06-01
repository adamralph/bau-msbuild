@ECHO OFF

ECHO [Dog] WARNING: Ensure you have built using Debug config (e.g in Visual Studio) before building with this!

ECHO [Dog] Copying Bau.MSBuild from bin\Debug...
xcopy src\Bau.MSBuild\bin\Debug\Bau.MSBuild.dll packages\Bau.MSBuild.0.1.0-beta03\lib\net45\ /Y /Q

ECHO [Dog] Running dog.csx...
scriptcs dog.csx -- %*
