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

## Running a bot against This

The bot must have a docker image / Dockerfile to run with this mode.

1. Build your dockerfile with `docker build --tag  {tag} .`
    - Where `{tag}` should be replaced with the name you want your docker image to be
2. Run your docker image with `docker run -it --network=ec_network -e RUNNER_IPV4=runner {tag}`
    - Where `{tag}` is the same as the tag used in step 1
