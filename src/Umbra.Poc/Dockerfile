FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ARG PUBLISH_PATH

COPY ${PUBLISH_PATH} .

ENTRYPOINT ["dotnet", "Umbra.Poc.dll"]
