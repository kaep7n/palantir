#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Palantir/Palantir.csproj", "Palantir/"]
RUN dotnet restore "Palantir/Palantir.csproj"
COPY . .
WORKDIR "/src/Palantir"
RUN dotnet build "Palantir.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Palantir.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Palantir.dll"]