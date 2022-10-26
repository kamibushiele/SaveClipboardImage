@REM readme, license等コピー
md $(TargetDir)Licenses
@REM xcopy /e /y $(ProjectDir)..\Licenses $(TargetDir)Licenses
xcopy /y $(SolutionDir)\License.txt $(TargetDir)
xcopy /y $(SolutionDir)\Readme.md $(TargetDir)
@REM 不要ファイル削除
@REM del $(TargetDir)*.xml
del $(TargetDir)*.pdb

@REM 実行ファイル配布用zip化。
@REM zipコマンドが使える場合は 以下gotoをコメントアウト
@REM goto END
cd $(TargetDir)\..\
md $(ProjectName)
echo aaa $(targetDir)
xcopy $(targetDir) /e /y $(ProjectName)\
zip -r $(ProjectName).zip $(ProjectName)\
rd /s /q $(ProjectName)\
:END