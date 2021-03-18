# Base image provided by Entelect Challenge
FROM entelectchallenge/dotnetcore:2021

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
# ' dotnet publish --configuration Release --output ./publish/ ' is used to build the .Net bot and to get that folder.
COPY ./publish/ .

# The entrypoint to run the bot
ENTRYPOINT ["dotnet", "ReferenceBot.dll"]