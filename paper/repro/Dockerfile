FROM mcr.microsoft.com/dotnet/sdk:10.0-noble

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

ENV DOTNET_CLI_HOME=/workspace/.dotnet \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    NUGET_PACKAGES=/workspace/.nuget/packages

WORKDIR /workspace

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl gnupg apt-transport-https \
    && curl -fsSL https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -o packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends powershell \
    && rm -rf /var/lib/apt/lists/*

COPY NuGet.Config RomanAI.slnx ./
COPY src/RomanAI.Core/ src/RomanAI.Core/
COPY tools/RomanAI.Headless/ tools/RomanAI.Headless/
COPY tests/RomanAI.Core.Tests/ tests/RomanAI.Core.Tests/
COPY docs/schemas/roman_validation_scenario_manifest.schema.json docs/schemas/roman_validation_scenario_manifest.schema.json
COPY docs/paper_track_research_protocol.md docs/paper_track_research_protocol.md
COPY docs/paper_track_systems_reproducibility_draft.md docs/paper_track_systems_reproducibility_draft.md
COPY Romana/Assets/Scripts/RomanAI/Core/ Romana/Assets/Scripts/RomanAI/Core/
COPY Romana/Tools/Test-PaperReadyBundle.ps1 Romana/Tools/Test-PaperReadyBundle.ps1

RUN dotnet restore RomanAI.slnx --configfile NuGet.Config
RUN dotnet build RomanAI.slnx --configfile NuGet.Config -m:1 -v:minimal

ENTRYPOINT ["pwsh", "-NoLogo", "-NoProfile", "-File", "/workspace/Romana/Tools/Test-PaperReadyBundle.ps1", "-ProjectPath", "/workspace/Romana", "-OutputPath", "/workspace/out/paper-ready-bundle"]
