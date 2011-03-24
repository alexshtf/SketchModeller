copy ..\..\source\HelixToolkit\bin\Release\*.dll lib
mkdir tmp
move content\.svn tmp
..\..\tools\nuget.exe pack HelixToolkit.nuspec
move tmp\.svn content
rmdir tmp
pause