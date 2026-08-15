#!/bin/sh
# Purpose: Verifies the baked private seed, creates the disposable writable store and starts only the public product host.
set -eu

seed_root="${RAG_CHALLENGE_SEED_ROOT:-/opt/rag-challenge/seed}"
runtime_store="${RAG_CHALLENGE_RUNTIME_STORE:-/tmp/rag-challenge-store}"
port="${PORT:-10000}"

if [ ! -f "${seed_root}/seed-manifest.sha256" ]; then
    echo "CH_DEPLOY_SEED_MANIFEST_MISSING" >&2
    exit 20
fi

(
    cd "${seed_root}"
    sha256sum -c seed-manifest.sha256 >/dev/null
) || {
    echo "CH_DEPLOY_SEED_INTEGRITY_FAILED" >&2
    exit 21
}

rm -rf -- "${runtime_store}"
mkdir -p -- "${runtime_store}"
cp -R -- "${seed_root}/control.db" "${runtime_store}/control.db"
cp -R -- "${seed_root}/vectors.db" "${runtime_store}/vectors.db"
cp -R -- "${seed_root}/content" "${runtime_store}/content"
chmod -R u+rwX -- "${runtime_store}"

(
    cd "${runtime_store}"
    sha256sum -c "${seed_root}/seed-manifest.sha256" >/dev/null
) || {
    echo "CH_DEPLOY_RUNTIME_STORE_INTEGRITY_FAILED" >&2
    exit 22
}

export ASPNETCORE_URLS="http://0.0.0.0:${port}"
export RagChallenge__Product__StoreRoot="${runtime_store}"

exec dotnet /app/RagChallenge.Server.Api.dll
