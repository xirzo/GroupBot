FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

COPY . /source

WORKDIR /source/GroupBot.Program

ARG TARGETARCH

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

RUN cp bot_data.db /app/ 2>/dev/null || echo "No bot_data.db file found, skipping."

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

RUN apk add --no-cache icu-libs

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

COPY --from=build /app .

RUN chmod -R 777 /app

USER $APP_UID

ENTRYPOINT ["dotnet", "GroupBot.Program.dll"]
