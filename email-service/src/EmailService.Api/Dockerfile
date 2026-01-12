# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/EmailService.Api/EmailService.Api.csproj ./src/EmailService.Api/
COPY src/EmailService.Application/EmailService.Application.csproj ./src/EmailService.Application/
COPY src/EmailService.Domain/EmailService.Domain.csproj ./src/EmailService.Domain/
COPY src/EmailService.Infrastructure/EmailService.Infrastructure.csproj ./src/EmailService.Infrastructure/
COPY src/EmailService.Shared/EmailService.Shared.csproj ./src/EmailService.Shared/
COPY global.json .
RUN dotnet restore ./src/EmailService.Api/EmailService.Api.csproj

COPY src ./src
RUN dotnet publish ./src/EmailService.Api/EmailService.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EmailService.Api.dll"]
