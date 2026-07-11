# Build from /src as the deployment root (SmartASP Root Directory = src)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Archive.Domain/Archive.Domain.csproj Archive.Domain/
COPY Archive.Contracts/Archive.Contracts.csproj Archive.Contracts/
COPY Archive.Application/Archive.Application.csproj Archive.Application/
COPY Archive.Infrastructure/Archive.Infrastructure.csproj Archive.Infrastructure/
COPY Archive.Api/Archive.Api.csproj Archive.Api/

RUN dotnet restore Archive.Api/Archive.Api.csproj

COPY Archive.Domain/ Archive.Domain/
COPY Archive.Contracts/ Archive.Contracts/
COPY Archive.Application/ Archive.Application/
COPY Archive.Infrastructure/ Archive.Infrastructure/
COPY Archive.Api/ Archive.Api/

RUN dotnet publish Archive.Api/Archive.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Archive.Api.dll"]
