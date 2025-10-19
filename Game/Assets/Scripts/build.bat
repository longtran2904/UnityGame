@echo off
ctime -begin vailoz.ctm
REM D:\Programs\2020.3.38f1\Editor\Unity.exe -batchmode -logFile - -projectPath "D:\Documents\GitHub\Rogue-like-game-i-guess\The game is liar" -quit -nographics
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" -nologo -warnAsMessage:CS0649 -clp:Summary;ErrorsOnly;WarningsOnly; -v:m "D:\Documents\GitHub\Rogue-like-game-i-guess\The game is liar\The game is liar.sln"
ctime -end vailoz.ctm