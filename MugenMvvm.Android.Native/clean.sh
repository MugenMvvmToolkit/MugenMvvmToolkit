#!/bin/bash -eu

BASEDIR=$(dirname "$0")
cd "$BASEDIR" || exit $?
bash ./gradlew clean