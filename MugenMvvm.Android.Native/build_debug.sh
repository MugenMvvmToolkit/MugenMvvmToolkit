#!/bin/bash -eu

localPath=$(dirname "$0")
projectPath=$localPath
copyFromPath=$projectPath/app/build/outputs/aar
copyToPath=$projectPath/../MugenMvvm.Platforms/Android/Jars

buildTask=assembleDebug

cd "$projectPath" || exit $?
./gradlew clean $buildTask

mkdir -p "$copyToPath"
cp -rf "$copyFromPath"/app-debug.aar "$copyToPath"/mugenmvvm-core.aar
