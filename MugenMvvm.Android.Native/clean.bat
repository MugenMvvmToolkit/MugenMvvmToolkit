SET localPath=%~dp0
SET projectPath=%localPath%

CD %projectPath%
call gradlew clean