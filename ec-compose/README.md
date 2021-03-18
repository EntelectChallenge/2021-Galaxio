# Engine Compose Repo

This repo provides a convenient way to load and run all the engine components together.

## How to Use

1. Grab the Build and Compose files
2. Put them alongside each of the 3 engine repos, such that:
    - build.sh
    - docker-compose.yaml
    - game-runner/
    - game-engine/
    - game-logger/

3. Run `build.sh`
4. Once complete, run `docker-compose -p "ec" up --build`
5. Once the run has completed you can use the following scripts to download the match logs
5.1 Match state: `docker cp ec_logger_1:/app/matchState.log ./matchState.json`
5.2 Game Complete: `docker cp ec_logger_1:/app/gameComplete.log ./gameComplete.json`

## Running a bot against This

The bot must have a docker image / Dockerfile to run with this mode.

1. Build your dockerfile with `docker build --tag  {tag} .`
    - Where `{tag}` should be replaced with the name you want your docker image to be
2. Run your docker image with `docker run -it --network=ec_network -e RUNNER_IPV4=runner {tag}`
    - Where `{tag}` is the same as the tag used in step 1

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
