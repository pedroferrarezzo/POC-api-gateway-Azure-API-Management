# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app
COPY . ./

RUN dotnet restore simpleauthapi.csproj
RUN dotnet publish -c Release -o out simpleauthapi.csproj

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app
COPY --from=build /app/out ./

ARG EXPOSE_PORT
EXPOSE ${EXPOSE_PORT}

ENTRYPOINT ["dotnet", "simpleauthapi.dll"]