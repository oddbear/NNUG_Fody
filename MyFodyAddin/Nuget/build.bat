@echo off

set lib=
set addin=MyFodyAddin.Fody

REM https://docs.microsoft.com/en-us/nuget/reference/package-versioning
dotnet build .. -c Release

REM Clean folders
del /f/s/q pack\lib
del /f/s/q pack\netclassicweaver
del /f/s/q pack\netstandardweaver

REM Copy files
(for %%a in (%addin%) do (
   copy /Y ..\MyFodyAddin.Fody\bin\Release\net46\%%a%.dll .\pack\netclassicweaver\
   copy /Y ..\MyFodyAddin.Fody\bin\Release\net46\%%a%.pdb .\pack\netclassicweaver\
   copy /Y ..\MyFodyAddin.Fody\bin\Release\netstandard2.0\%%a%.dll .\pack\netstandardweaver\
   copy /Y ..\MyFodyAddin.Fody\bin\Release\netstandard2.0\%%a%.pdb .\pack\netstandardweaver\
))

