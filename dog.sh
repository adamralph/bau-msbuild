#!/usr/bin/env bash
set -e
set -o pipefail
set -x

cp src/Bau.MSBuild/bin/Release/Bau.MSBuild.dll scriptcs_packages/Bau.MSBuild.0.1.0-beta01/lib/net45/ 
scriptcs ./baufile.csx -- $@
