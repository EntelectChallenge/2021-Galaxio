# Base image provided by Entelect Challenge
FROM entelectchallenge/java:2021

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY ./publish/ .

# The entrypoint to run the bot
ENTRYPOINT ["java", "-jar", /JavaBot.jar]