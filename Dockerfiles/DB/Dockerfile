FROM mcr.microsoft.com/mssql/server:2019-latest

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=<YourStrong@Passw0rd>

COPY ["Scripts/*", "Db/Scripts/"]
VOLUME ./Db:/var/opt/mssql/data

HEALTHCHECK --interval=10s --timeout=5s --start-period=10s --retries=10 \
    CMD /opt/mssql-tools/bin/sqlcmd -S . -U sa -P <YourStrong@Passw0rd> -i Db/Scripts/SetupDb.sql || exit 1