#!/bin/bash

cd ./runner-publish/ && dotnet GameRunner.dll &
cd ./engine-publish/ && dotnet Engine.dll &
cd ./logger-publish/ && dotnet Logger.dll &
cd ./reference-bot-publish/ && dotnet ReferenceBot.dll &
cd ./reference-bot-publish/ && dotnet ReferenceBot.dll &
cd ./reference-bot-publish/ && dotnet ReferenceBot.dll &

$SHELL