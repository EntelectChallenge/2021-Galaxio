#!/bin/bash

mkdir starter-pack

cp -r ./engine-publish ./starter-pack/
cp -r ./runner-publish ./starter-pack/
cp -r ./logger-publish ./starter-pack/
cp -r ./reference-bot-publish ./starter-pack/
cp -r ./starter-bots ./starter-pack/
cp -r ./visualiser ./starter-pack/

cp ./starter-bots/README.md ./starter-pack/
cp ./building-a-bot.md ./starter-pack/
cp ./run.sh ./starter-pack/