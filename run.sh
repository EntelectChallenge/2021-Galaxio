#!/bin/bash

cd ../ec-compose
./build.sh
docker-compose -p "ec" up --build