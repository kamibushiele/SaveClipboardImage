@REM readme, license���R�s�[
md $(TargetDir)Licenses
xcopy /y $(SolutionDir)\Licenses\* $(TargetDir)Licenses
xcopy /y $(SolutionDir)\License.txt $(TargetDir)
xcopy /y $(SolutionDir)\Readme.md $(TargetDir)
xcopy /y $(ProjectDir)\bat\* $(TargetDir)
@REM �s�v�t�@�C���폜
@REM del $(TargetDir)*.xml
del $(TargetDir)*.pdb

@REM ���s�t�@�C���z�z�pzip���B
@REM zip�R�}���h���g����ꍇ�� �ȉ�goto���R�����g�A�E�g
@REM goto END
cd $(TargetDir)\..\
md $(ProjectName)
echo aaa $(targetDir)
xcopy $(targetDir) /e /y $(ProjectName)\
zip -r $(ProjectName).zip $(ProjectName)\
rd /s /q $(ProjectName)\
:END