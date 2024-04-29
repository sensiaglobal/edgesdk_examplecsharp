FROM qratehcc2sdk.azurecr.io/dotnet-runtime:6.0-2023-03-02-patched-proddebug AS base
# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN useradd -d /app -M -s /sbin/nologin -u 5678 -U appuser
USER appuser:appuser

FROM 	qratehcc2sdk.azurecr.io/dotnet-sdk:6.0-2023-03-02-build AS build
WORKDIR /src

COPY csapp/csapp.csproj csapp/
COPY csapp/nuget.config csapp/
COPY csapp/SDKPackages/*.nupkg csapp/SDKPackages/

RUN dotnet restore "csapp/csapp.csproj" 
COPY . /src
WORKDIR /src/csapp
RUN dotnet build "csapp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "csapp.csproj" -c Release -o /app/publish

FROM base AS slim
# Environment variable needed for read-only filesystem
ENV COMPlus_EnableDiagnostics=0
WORKDIR /app
COPY --from=publish --chown=appuser:appuser /app/publish /app
COPY --chown=appuser:appuser csapp/appsettings.json /app
COPY --chown=appuser:appuser csapp/appconfig/ /app/appconfig/
USER root:root
RUN install -o appuser -g appuser -d -m 0755 /temp
RUN install -o appuser -g appuser -d -m 0755 /app/appconfig
USER appuser:appuser
VOLUME [ "/temp" ]
ENTRYPOINT ["dotnet", "csapp.dll"]

LABEL org.opencontainers.image.authors="NGRTU Team <ngrtuteam@sensiaglobal.com>" \
      org.opencontainers.image.vendor="Sensia Global" \
      org.opencontainers.image.url="https://www.sensiaglobal.com/" \
      org.opencontainers.image.license="Propietary" \
      com.sensiaglobal.image.artifacts.source="sensia-edge-docker-dev"



