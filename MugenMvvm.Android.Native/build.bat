SET localPath=%~dp0
SET projectPath=%localPath%
SET copyFromPath=%projectPath%\app\build\outputs\aar
SET copyToPath=%localPath%..\MugenMvvm.Platforms\Android\Jars

SET buildTask=assembleRelease

CD %projectPath%
call gradlew %buildTask%

copy /y %copyFromPath%\app-release.aar %copyToPath%\mugenmvvm-core.aar