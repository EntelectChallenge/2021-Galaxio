FROM entelectchallenge/cplusplus:2021

WORKDIR /app

ARG DOCKER_BUILD

COPY ./ .

RUN DOCKER_BUILD=$DOCKER_BUILD cmake -S . -B ./publish
RUN cmake --build ./publish

WORKDIR ./publish

ENTRYPOINT ["./cppBot"]

