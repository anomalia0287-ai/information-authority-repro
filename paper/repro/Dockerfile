# Base image pinned by digest for long-term reproducibility: the moving tag
# mcr.microsoft.com/dotnet/sdk:10.0-noble currently resolves to the manifest-list
# digest below (CI resolves the linux/amd64 child). Pinning freezes the .NET SDK
# that governs the numeric outputs so future tag updates cannot silently change them.
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble@sha256:c0790639332692a0d56cdd81ed581cfd24d040d9839764c138994866df89a3b6

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

ENV DOTNET_CLI_HOME=/workspace/.dotnet \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    NUGET_PACKAGES=/workspace/.nuget/packages

WORKDIR /workspace

# PowerShell here is only the test-harness runner that invokes the .NET verifier; it is
# intentionally not version-pinned. The reproduced numbers come from the digest-pinned
# .NET SDK above, and pinning an apt version that may later be dropped from the upstream
# feed would itself break the build and thus reproducibility.
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
