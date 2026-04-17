FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Car4rentpg/*.csproj Car4rentpg/
RUN dotnet restore

COPY Car4rentpg/. Car4rentpg/
WORKDIR /src/Car4rentpg
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Car4rentpg.dll"]