# Base image provided by Entelect Challenge
FROM entelectchallenge/python:2021

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY ./ .

# The entrypoint to run the bot
ENTRYPOINT ["python", "StarterBot.py"]