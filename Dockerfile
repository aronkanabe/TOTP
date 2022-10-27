FROM mcr.microsoft.com/dotnet/aspnet:6.0.9 AS base
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:6.0.400 AS build
WORKDIR /src

COPY ["TOTP.Core/TOTP.Core.csproj", "TOTP.Core/"]
RUN dotnet restore "TOTP.Core/TOTP.Core.csproj" -r linux-musl-x64

COPY ["TOTP.Infrastructure/TOTP.Infrastructure.csproj", "TOTP.Infrastructure/"]
RUN dotnet restore "TOTP.Infrastructure/TOTP.Infrastructure.csproj" -r linux-musl-x64

COPY ["TOTP.Application/TOTP.Application.csproj", "TOTP.Application/"]
RUN dotnet restore "TOTP.Application/TOTP.Application.csproj" -r linux-musl-x64

COPY ["TOTP.API/TOTP.API.csproj", "TOTP.API/"]
RUN dotnet restore "TOTP.API/TOTP.API.csproj" -r linux-musl-x64

COPY ["TOTP.Main/TOTP.Main.csproj", "TOTP.Main/"]
RUN dotnet restore "TOTP.Main/TOTP.Main.csproj" -r linux-musl-x64

COPY . .
WORKDIR "/src/TOTP.Main"
RUN dotnet build "TOTP.Main.csproj" --no-restore -r linux-musl-x64 --self-contained -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TOTP.Main.csproj" --no-restore -r linux-musl-x64 --self-contained -p:PublishSingleFile=true -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0.9-alpine3.16 as final
EXPOSE 5000
EXPOSE 5001
RUN addgroup -S totpgroup && \
    adduser -S totpuser
USER totpuser
COPY --from=publish --chown=totpuser:totpgroup /app/publish/appsettings.json appsettings.json
COPY --from=publish --chown=totpuser:totpgroup /app/publish/TOTP.Main TOTP.Main
CMD ["./TOTP.Main"]

