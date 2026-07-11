# Build the full layered solution (Api + Application + Domain + Infrastructure + Contracts)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Archive.Domain/Archive.Domain.csproj Archive.Domain/
COPY src/Archive.Contracts/Archive.Contracts.csproj Archive.Contracts/
COPY src/Archive.Application/Archive.Application.csproj Archive.Application/
COPY src/Archive.Infrastructure/Archive.Infrastructure.csproj Archive.Infrastructure/
COPY src/Archive.Api/Archive.Api.csproj Archive.Api/

RUN dotnet restore Archive.Api/Archive.Api.csproj

COPY src/Archive.Domain/ Archive.Domain/
COPY src/Archive.Contracts/ Archive.Contracts/
COPY src/Archive.Application/ Archive.Application/
COPY src/Archive.Infrastructure/ Archive.Infrastructure/
COPY src/Archive.Api/ Archive.Api/

RUN dotnet publish Archive.Api/Archive.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
# Render injects PORT; default 8080 for local docker runs
ENV PORT=8080
EXPOSE 8080

CMD ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet Archive.Api.dll
