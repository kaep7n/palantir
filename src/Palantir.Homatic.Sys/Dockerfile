#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Palantir.Homatic.Sys/Palantir.Homatic.Sys.csproj", "Palantir.Homatic.Sys/"]
RUN dotnet restore "Palantir.Homatic.Sys/Palantir.Homatic.Sys.csproj"
COPY . .
WORKDIR "/src/Palantir.Homatic.Sys"
RUN dotnet build "Palantir.Homatic.Sys.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Palantir.Homatic.Sys.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Palantir.Homatic.Sys.dll"]