#!/bin/bash

cd ../game-engine
dotnet publish --configuration Release --output ./publish/

cd ../game-logger
dotnet publish --configuration Release --output ./publish/

cd ../game-runner
dotnet publish --configuration Release --output ./publish/
