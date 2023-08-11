#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
WORKDIR /src
COPY ["Palantir.Homatic.Mock/Palantir.Homatic.Mock.csproj", "Palantir.Homatic.Mock/"]
RUN dotnet restore "Palantir.Homatic.Mock/Palantir.Homatic.Mock.csproj"
COPY . .
WORKDIR "/src/Palantir.Homatic.Mock"
RUN dotnet build "Palantir.Homatic.Mock.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Palantir.Homatic.Mock.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Palantir.Homatic.Mock.dll"]