FROM mcr.microsoft.com/dotnet/runtime-deps:5.0.6

LABEL maintainer "Digitaal Vlaanderen <digitaal.vlaanderen@vlaanderen.be>"
LABEL registry="road-registry"

COPY / /app
WORKDIR /app

RUN apt-get update && \
    apt-get install curl jq -y && \
    chmod +x ./init.sh

ENTRYPOINT ["./init.sh"]
