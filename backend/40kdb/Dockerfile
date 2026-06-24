FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /src/../data/ /seed-data/

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5050
ENV ASPNETCORE_URLS=http://+:5050

ENTRYPOINT ["dotnet", "40kdb.dll"]
