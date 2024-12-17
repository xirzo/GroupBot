FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/GroupBot/GroupBot.csproj ./src/GroupBot/
RUN dotnet restore ./src/GroupBot/GroupBot.csproj

COPY src/ ./src/

WORKDIR /app/src/GroupBot
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "GroupBot.dll"]
