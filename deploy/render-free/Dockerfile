# Purpose: Runs the prebuilt RAG-Challenge release with an immutable private seed and a fresh ephemeral writable store on Render Free.
FROM mcr.microsoft.com/dotnet/aspnet:10.0.11@sha256:207cc51496778557731c81ff670333d8ade4a4fec22768fd1be8e78474a84ecf

USER root
WORKDIR /app

COPY release/ ./
COPY seed/ /opt/rag-challenge/seed/
COPY entrypoint.sh /usr/local/bin/rag-challenge-entrypoint

RUN chmod 0555 /usr/local/bin/rag-challenge-entrypoint \
    && chmod -R a-w /opt/rag-challenge/seed \
    && chown -R app:app /app

USER app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    RAG_CHALLENGE_SEED_ROOT=/opt/rag-challenge/seed \
    RAG_CHALLENGE_RUNTIME_STORE=/tmp/rag-challenge-store

EXPOSE 10000

ENTRYPOINT ["/usr/local/bin/rag-challenge-entrypoint"]
