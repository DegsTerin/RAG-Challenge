#!/bin/sh
# Purpose: Verifies the baked private seed, creates the disposable writable store and starts only the public product host.
set -eu

seed_root="/opt/rag-challenge/seed"
runtime_parent="/tmp"
runtime_store="/tmp/rag-challenge-store"
runtime_marker=".rag-challenge-runtime-store-v1"
runtime_marker_value="rag-challenge-runtime-store-v1"
port="${PORT:-10000}"

if [ "${RAG_CHALLENGE_SEED_ROOT:-${seed_root}}" != "${seed_root}" ] ||
    [ "${RAG_CHALLENGE_RUNTIME_STORE:-${runtime_store}}" != "${runtime_store}" ] ||
    [ ! -d "${runtime_parent}" ] ||
    [ -L "${runtime_parent}" ] ||
    [ -L "${seed_root}" ]; then
    echo "CH_DEPLOY_RUNTIME_STORE_UNSAFE" >&2
    exit 23
fi

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

if [ -e "${runtime_store}" ] || [ -L "${runtime_store}" ]; then
    if [ ! -d "${runtime_store}" ] ||
        [ -L "${runtime_store}" ] ||
        [ ! -f "${runtime_store}/${runtime_marker}" ] ||
        [ -L "${runtime_store}/${runtime_marker}" ] ||
        [ "$(cat "${runtime_store}/${runtime_marker}" 2>/dev/null)" != "${runtime_marker_value}" ]; then
        echo "CH_DEPLOY_RUNTIME_STORE_UNSAFE" >&2
        exit 24
    fi

    rm -rf -- "${runtime_store}"
fi

umask 077
mkdir -p -- "${runtime_store}"
printf '%s\n' "${runtime_marker_value}" > "${runtime_store}/${runtime_marker}"
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
