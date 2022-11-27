#!/bin/bash

# Echo everythiny
set -x
# Immediately exit the script when a command ends with an error
set -e

dotnet.exe clean

rm -rf zip
mkdir zip

cp .dockerignore zip/
cp .editorconfig zip/
cp .gitignore zip/
cp docker-compose.yml zip/
cp Dockerfile zip/
cp InterviewService.sln zip/
cp NuGet.Config zip/
cp README.md zip/

cp -r ./Client zip/
cp -r ./packages zip/
cp -r ./Source zip/
cp -r ./Tests zip/

echo "done!"
read -p "Press any key to end"
