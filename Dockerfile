# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY src/GroupBot/GroupBot.csproj ./src/GroupBot/
RUN dotnet restore ./src/GroupBot/GroupBot.csproj

# Copy the entire source code
COPY src/ ./src/

# Build and publish the application
WORKDIR /app/src/GroupBot
RUN dotnet publish -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the application port and set the entry point
EXPOSE 80
ENTRYPOINT ["dotnet", "GroupBot.dll"]
