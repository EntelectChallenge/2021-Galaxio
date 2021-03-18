# Base image provided by Entelect Challenge
FROM public.ecr.aws/m5z5a5b2/languages/java:2021

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY ./target/ .

# The entrypoint to run the bot
ENTRYPOINT ["java", "-jar", "JavaBot.jar"]
