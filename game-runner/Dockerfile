FROM mcr.microsoft.com/dotnet/aspnet:3.1-alpine

WORKDIR /app
COPY ./publish/ .

EXPOSE 5000

CMD ["dotnet", "GameRunner.dll"]