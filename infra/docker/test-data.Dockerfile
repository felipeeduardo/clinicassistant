FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /workspace
RUN apt-get update && apt-get install -y --no-install-recommends postgresql-client && rm -rf /var/lib/apt/lists/*
COPY backend/ .
COPY scripts/test-data/ scripts/test-data/
RUN dotnet restore tools/ClinicAssistant.TestDataHash/ClinicAssistant.TestDataHash.csproj && dotnet build tools/ClinicAssistant.TestDataHash/ClinicAssistant.TestDataHash.csproj --no-restore
RUN chmod +x scripts/test-data/*.sh
ENTRYPOINT ["/workspace/scripts/test-data/docker-entrypoint.sh"]
