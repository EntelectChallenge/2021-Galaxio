#!/bin/bash

cd ./runner-publish/ && dotnet GameRunner.dll &
cd ./engine-publish/ && sleep 1 && dotnet Engine.dll &
cd ./logger-publish/ && sleep 1 && dotnet Logger.dll &
cd ./reference-bot-publish/ && sleep 3 && dotnet ReferenceBot.dll &
cd ./reference-bot-publish/ && sleep 3 && dotnet ReferenceBot.dll &
cd ./reference-bot-publish/ && sleep 3 && dotnet ReferenceBot.dll &

wait