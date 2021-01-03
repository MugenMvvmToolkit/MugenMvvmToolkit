SET localPath=%~dp0
SET projectPath=%localPath%
SET copyFromPath=%projectPath%\app\build\outputs\aar
SET copyToPath=%localPath%..\MugenMvvm.Platforms\Android\Jars

SET buildTask=assembleDebug

CD %projectPath%
call gradlew %buildTask%

copy /y %copyFromPath%\app-debug.aar %copyToPath%\mugenmvvm-core.aar