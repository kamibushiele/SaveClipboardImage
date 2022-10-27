call %~dp0path.bat
reg add %target% /f /d クリップボード画像を保存する
reg add %targetCmd% /f /d "%exe% -o %%V"
pause