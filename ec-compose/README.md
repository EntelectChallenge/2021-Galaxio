# Engine Compose Repo

This repo provides a convenient way to load and run all the engine components together.

## Pre-requisites

- .NET 5
    - https://dotnet.microsoft.com/download/dotnet/5.0
- Docker
    - https://www.docker.com/products/docker-desktop

## How to Use

1. Clone the entire 2021-Galaxio Github Repository
2. Ensure the following folder structure on your drive:
    - ec-compose/
    - game-runner/
    - game-engine/
    - game-logger/
    - starter-bots/
3. Open the `ec-compose` folder.
4. Inside this folder, run `build.sh`
5. Once complete, open a terminal or command prompt, and run `docker-compose -p "ec" up --build`
6. Once the run, it will take a moment to shut down, and then you can use the following commands to download the match logs
    1. Match state: `docker cp ec_logger_1:/app/matchState.log.json ./matchState.json`
    2. Game Complete: `docker cp ec_logger_1:/app/gameComplete.log.json ./matchState_gameComplete.json`

## Running a bot against This

### With Docker

1. Build your dockerfile with `docker build --tag  {tag} .`
    - Where `{tag}` should be replaced with the name you want your docker image to be
2. Run your docker image with `docker run -it --network=ec_network -e RUNNER_IPV4=runner {tag}`
    - Where `{tag}` is the same as the tag used in step 1

### Otherwise

The runner that is running in the docker container has its ports exposed. To use this, simply run your bot however you prefer, and target `http://localhost:5000` when your bot tries to connect.

## Running multiple bots in the docker compose

You can test a game with multiple bots by completing the following

Example Bot Config:
``` ec_bot0:
        build: ../myEcBot
        networks:
            - network
        environment:
            RUNNER_IPV4: runner
        depends_on:
            - "runner"
            - "engine"
            - "logger"
```
Example Reference Bot Config:
``` ec_ref_bot1:
        build: ../starter-bots/ReferenceBot
        networks:
            - network
        environment:
            RUNNER_IPV4: runner
        depends_on:
            - "runner"
            - "engine"
            - "logger"
```

1. Create you bot config in the docker-compose.yml
2. Setup as many reference bots in your docker-compose.yml (as many as you have stipulated in the bot count)
3. Profit
