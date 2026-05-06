FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MervaApi/MervaApi.csproj MervaApi/
RUN dotnet restore MervaApi/MervaApi.csproj

COPY MervaApi/ MervaApi/
RUN dotnet publish MervaApi/MervaApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MervaApi.dll"]
