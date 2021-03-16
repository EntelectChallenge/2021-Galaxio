#!/bin/bash

dotnet ./runner-publish/GameRunner.dll &
dotnet ./engine-publish/Engine.dll &
dotnet ./logger-publish/Logger.dll &
dotnet ./reference-bot-publish/Logger.dll &
dotnet ./reference-bot-publish/Logger.dll &
dotnet ./reference-bot-publish/Logger.dll &

$SHELL