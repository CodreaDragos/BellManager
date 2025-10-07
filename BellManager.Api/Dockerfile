# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the .NET 9 SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BellManager.Api.csproj", "."]
RUN dotnet restore "BellManager.Api.csproj"
COPY . .
RUN dotnet build "BellManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BellManager.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BellManager.Api.dll"]
