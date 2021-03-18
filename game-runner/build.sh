#!/bin/bash

cd ./game-engine
dotnet publish --configuration Release --output ../engine-publish/

cd ../game-logger
dotnet publish --configuration Release --output ../logger-publish/

cd ../game-runner
dotnet publish --configuration Release --output ../runner-publish/
