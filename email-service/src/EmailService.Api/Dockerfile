# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./EmailService.Api/EmailService.Api.csproj ./EmailService.Api/
COPY ../EmailService.Application/EmailService.Application.csproj ./EmailService.Application/
COPY ../EmailService.Domain/EmailService.Domain.csproj ./EmailService.Domain/
COPY ../EmailService.Infrastructure/EmailService.Infrastructure.csproj ./EmailService.Infrastructure/
COPY ../EmailService.Shared/EmailService.Shared.csproj ./EmailService.Shared/
RUN dotnet restore ./EmailService.Api/EmailService.Api.csproj

COPY ./EmailService.Api ./EmailService.Api
COPY ../EmailService.Application ./EmailService.Application
COPY ../EmailService.Domain ./EmailService.Domain
COPY ../EmailService.Infrastructure ./EmailService.Infrastructure
COPY ../EmailService.Shared ./EmailService.Shared
RUN dotnet publish ./EmailService.Api/EmailService.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EmailService.Api.dll"]
