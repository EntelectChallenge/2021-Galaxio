# Base image provided by Entelect Challenge
FROM public.ecr.aws/m5z5a5b2/languages/cplusplus:2021

WORKDIR /app

ARG DOCKER_BUILD

COPY ./ .

RUN DOCKER_BUILD=$DOCKER_BUILD cmake -S . -B ./publish
RUN cmake --build ./publish

WORKDIR ./publish

# The entrypoint to run the bot
ENTRYPOINT ["./cppBot"]

