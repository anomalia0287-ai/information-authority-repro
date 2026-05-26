param(
    [string]$ProjectPath = (Resolve-Path "$PSScriptRoot\..").Path,
    [string]$OutputPath = "",
    [int]$Repetitions = 30,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

if ($Repetitions -lt 30) {
    throw "Repetitions must be >= 30 for paper-ready verification."
}

$ProjectPath = (Resolve-Path $ProjectPath).Path
$repoRoot = Split-Path -Parent $ProjectPath
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot ".headless-runs\paper-ready-bundle"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repoRoot $OutputPath
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

$checks = @()
$warnings = @()
$errors = @()

function Add-Check {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Summary,
        [object]$Details = $null
    )

    $script:checks += [ordered]@{
        name = $Name
        passed = $Passed
        summary = $Summary
        details = $Details
    }

    if (-not $Passed) {
        $script:errors += "$Name`: $Summary"
    }
}

function Format-CommandLine {
    param(
        [string]$Command,
        [string[]]$Arguments
    )

    $parts = @($Command)
    foreach ($argument in @($Arguments)) {
        if ($argument -match '\s') {
            $parts += '"' + $argument + '"'
        }
        else {
            $parts += $argument
        }
    }

    return $parts -join ' '
}

function Invoke-PaperBundleStep {
    param(
        [string]$Name,
        [string]$Command,
        [string[]]$Arguments
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $script:LASTEXITCODE = 0
    Push-Location $repoRoot
    try {
        $ErrorActionPreference = "Continue"
        $output = & $Command @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
        Pop-Location
    }

    $outputLines = @($output | ForEach-Object { [string]$_ })
    $summary = if ($exitCode -eq 0) {
        "passed"
    }
    else {
        ($outputLines | Select-Object -Last 20 | Out-String).Trim()
    }

    Add-Check `
        -Name $Name `
        -Passed ($exitCode -eq 0) `
        -Summary $summary `
        -Details @{
            command = Format-CommandLine -Command $Command -Arguments $Arguments
            exitCode = $exitCode
            outputLineCount = $outputLines.Count
            outputTail = @($outputLines | Select-Object -Last 20)
        }

    return $exitCode
}

function Set-LocalDotnetEnvironment {
    $env:DOTNET_CLI_HOME = Join-Path $repoRoot ".dotnet"
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
    $env:NUGET_PACKAGES = Join-Path $repoRoot ".nuget\packages"
    $env:APPDATA = Join-Path $repoRoot ".appdata"
}

function Get-RelativeBundlePath {
    param([string]$RelativePath)
    return Join-Path $OutputPath $RelativePath
}

function Test-ExpectedFiles {
    $expectedFiles = @(
        "paper_results_table.md",
        "paper_limitations.md",
        "paper_readiness_report.csv",
        "paper_readiness_report.json",
        "paper_artifact_bundle_manifest.json",
        "default_experiment\paper_experiment_summary.csv",
        "default_experiment\paper_experiment_runs.csv",
        "default_experiment\paper_experiment_negative_controls.csv",
        "default_experiment\paper_experiment_failure_taxonomy.csv",
        "default_experiment\paper_experiment_result.json",
        "heldout_negative_experiment\paper_experiment_summary.csv",
        "heldout_negative_experiment\paper_experiment_runs.csv",
        "heldout_negative_experiment\paper_experiment_negative_controls.csv",
        "heldout_negative_experiment\paper_experiment_failure_taxonomy.csv",
        "heldout_negative_experiment\paper_experiment_result.json",
        "broader_unseen_experiment\paper_experiment_summary.csv",
        "broader_unseen_experiment\paper_experiment_runs.csv",
        "broader_unseen_experiment\paper_experiment_negative_controls.csv",
        "broader_unseen_experiment\paper_experiment_failure_taxonomy.csv",
        "broader_unseen_experiment\paper_experiment_result.json",
        "authority_no_cheat_experiment\paper_experiment_summary.csv",
        "authority_no_cheat_experiment\paper_experiment_runs.csv",
        "authority_no_cheat_experiment\paper_experiment_negative_controls.csv",
        "authority_no_cheat_experiment\paper_experiment_failure_taxonomy.csv",
        "authority_no_cheat_experiment\paper_experiment_result.json"
    )
    $missing = @()
    $empty = @()

    foreach ($fileName in $expectedFiles) {
        $path = Get-RelativeBundlePath -RelativePath $fileName
        if (-not (Test-Path -LiteralPath $path)) {
            $missing += $fileName
            continue
        }

        $item = Get-Item -LiteralPath $path
        if ($item.Length -le 0) {
            $empty += $fileName
        }
    }

    Add-Check `
        -Name "paper_bundle_files" `
        -Passed ($missing.Count -eq 0 -and $empty.Count -eq 0) `
        -Summary "expected files present and non-empty" `
        -Details @{
            expectedFiles = $expectedFiles
            missing = $missing
            empty = $empty
        }
}

function Test-ManifestHashes {
    $manifestPath = Get-RelativeBundlePath -RelativePath "paper_artifact_bundle_manifest.json"
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        Add-Check -Name "paper_bundle_manifest_hashes" -Passed $false -Summary "manifest missing" -Details @{ path = $manifestPath }
        return
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $issues = @()
    foreach ($file in @($manifest.files)) {
        $relativePath = [string]$file.path
        $path = Get-RelativeBundlePath -RelativePath ($relativePath -replace '/', '\')
        if (-not (Test-Path -LiteralPath $path)) {
            $issues += "missing:$relativePath"
            continue
        }

        $item = Get-Item -LiteralPath $path
        if ([int64]$file.bytes -ne $item.Length) {
            $issues += "size_mismatch:$relativePath"
        }

        $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash.ToLowerInvariant()
        if ($hash -ne ([string]$file.sha256).ToLowerInvariant()) {
            $issues += "sha256_mismatch:$relativePath"
        }
    }

    Add-Check `
        -Name "paper_bundle_manifest_hashes" `
        -Passed ($issues.Count -eq 0) `
        -Summary "bundle manifest hashes match generated files" `
        -Details @{
            manifestPath = $manifestPath
            issueCount = $issues.Count
            issues = $issues
        }
}

function Test-ArtifactCommands {
    $manifestPath = Get-RelativeBundlePath -RelativePath "paper_artifact_bundle_manifest.json"
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        Add-Check -Name "paper_bundle_command_provenance" -Passed $false -Summary "manifest missing" -Details @{ path = $manifestPath }
        return
    }

    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $commands = @($manifest.commands | ForEach-Object { [string]$_ })
    $issues = @()

    if ($commands.Count -lt 4) {
        $issues += "command_count"
    }

    if (($commands -join "`n").IndexOf("--paper-seed-start", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_seed_start"
    }

    if (($commands -join "`n").IndexOf($OutputPath, [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_output_path"
    }

    if (($commands -join "`n").IndexOf("--paper-heldout-manifest", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_heldout_manifest_arg"
    }

    if (($commands -join "`n").IndexOf("--paper-broader-manifest", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_broader_manifest_arg"
    }

    Add-Check `
        -Name "paper_bundle_command_provenance" `
        -Passed ($issues.Count -eq 0) `
        -Summary "artifact commands record actual seed, output path, and manifest arguments" `
        -Details @{
            manifestPath = $manifestPath
            issueCount = $issues.Count
            issues = $issues
            commands = $commands
        }
}

function Test-ReadinessReport {
    $readinessPath = Get-RelativeBundlePath -RelativePath "paper_readiness_report.json"
    if (-not (Test-Path -LiteralPath $readinessPath)) {
        Add-Check -Name "paper_readiness_report" -Passed $false -Summary "readiness report missing" -Details @{ path = $readinessPath }
        return
    }

    $report = Get-Content -LiteralPath $readinessPath -Raw | ConvertFrom-Json
    $gateMap = @{}
    foreach ($gate in @($report.gates)) {
        $gateMap[[string]$gate.gateId] = $gate
    }

    $requiredPassingGates = @(
        "research_question_fixed",
        "baseline_ablation_matrix_implemented",
        "held_out_manifest_implemented",
        "statistical_repetition_runner",
        "confidence_intervals_reported",
        "negative_controls_fail_closed",
        "failure_taxonomy_populated",
        "no_cheat_invariant_evidence",
        "artifact_bundle_generated",
        "limitations_section_present",
        "broader_unseen_scenario_families",
        "clean_machine_reproduction_script",
        "paper_narrative_draft_present"
    )
    $issues = @()
    foreach ($gateId in $requiredPassingGates) {
        if (-not $gateMap.ContainsKey($gateId) -or -not [bool]$gateMap[$gateId].passed) {
            $issues += "expected_pass:$gateId"
        }
    }

    foreach ($gateId in @("independent_clean_machine_reproduction", "paper_narrative_external_review")) {
        if (-not $gateMap.ContainsKey($gateId) -or [bool]$gateMap[$gateId].passed) {
            $issues += "expected_blocked:$gateId"
        }
    }

    if (-not [bool]$report.evidenceBundlePassed) {
        $issues += "evidenceBundlePassed_false"
    }

    if ([bool]$report.paperCompletionReady) {
        $issues += "paperCompletionReady_overclaim"
    }

    Add-Check `
        -Name "paper_readiness_report" `
        -Passed ($issues.Count -eq 0) `
        -Summary "readiness gates match expected paper-track state" `
        -Details @{
            readinessPath = $readinessPath
            issueCount = $issues.Count
            issues = $issues
        }
}

function Test-NegativeControls {
    $runsPath = Get-RelativeBundlePath -RelativePath "heldout_negative_experiment\paper_experiment_runs.csv"
    $negativeControlsPath = Get-RelativeBundlePath -RelativePath "heldout_negative_experiment\paper_experiment_negative_controls.csv"
    $taxonomyPath = Get-RelativeBundlePath -RelativePath "heldout_negative_experiment\paper_experiment_failure_taxonomy.csv"
    $issues = @()

    foreach ($path in @($runsPath, $negativeControlsPath, $taxonomyPath)) {
        if (-not (Test-Path -LiteralPath $path)) {
            $issues += "missing:$path"
        }
    }

    $runs = if (Test-Path -LiteralPath $runsPath) { Get-Content -LiteralPath $runsPath -Raw } else { "" }
    $negativeControls = if (Test-Path -LiteralPath $negativeControlsPath) { Get-Content -LiteralPath $negativeControlsPath -Raw } else { "" }
    $taxonomy = if (Test-Path -LiteralPath $taxonomyPath) { Get-Content -LiteralPath $taxonomyPath -Raw } else { "" }

    if ($runs.IndexOf("negative_control,mutation_violation,paper_negative_control_runner", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_runner_level_mutation_row"
    }

    if ($runs.IndexOf("negative_control,determinism_mismatch,paper_negative_control_runner", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_runner_level_determinism_row"
    }

    if ($runs.IndexOf("negative_control,privileged_truth_violation,paper_negative_control_runner", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_runner_level_authority_row"
    }

    if ($negativeControls.IndexOf("privileged_truth_violation,authority_failure,1,1,1.000000,true", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_authority_failure_summary"
    }

    if ($negativeControls.IndexOf("runner_level_failed_execution", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_runner_level_summary_source"
    }

    if ($taxonomy.IndexOf("negative_control", [System.StringComparison]::Ordinal) -lt 0) {
        $issues += "missing_negative_control_taxonomy_source"
    }

    Add-Check `
        -Name "paper_negative_controls" `
        -Passed ($issues.Count -eq 0) `
        -Summary "negative controls are backed by runner-level failed rows" `
        -Details @{
            issueCount = $issues.Count
            issues = $issues
        }
}

function Test-ClaimBoundaries {
    $limitationsPath = Get-RelativeBundlePath -RelativePath "paper_limitations.md"
    $resultsPath = Get-RelativeBundlePath -RelativePath "paper_results_table.md"
    $issues = @()
    $combinedContent = ""
    foreach ($path in @($limitationsPath, $resultsPath)) {
        if (-not (Test-Path -LiteralPath $path)) {
            $issues += "missing:$path"
            continue
        }

        $combinedContent += "`n" + (Get-Content -LiteralPath $path -Raw)
    }

    foreach ($required in @(
        "do not support",
        "tactical superiority",
        "human-realistic behavior validity",
        "movement/combat",
        "medical",
        "architecture-enforcement evidence",
        "privileged-truth"
    )) {
        if ($combinedContent.IndexOf($required, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            $issues += "missing_claim_boundary:$required"
        }
    }

    Add-Check `
        -Name "paper_claim_boundaries" `
        -Passed ($issues.Count -eq 0) `
        -Summary "result and limitation files preserve claim boundaries" `
        -Details @{
            issueCount = $issues.Count
            issues = $issues
        }
}

Set-LocalDotnetEnvironment
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

if (-not $SkipBuild) {
    Invoke-PaperBundleStep `
        -Name "dotnet_build" `
        -Command "dotnet" `
        -Arguments @("build", "RomanAI.slnx", "--configfile", "NuGet.Config", "-m:1", "-v:minimal") | Out-Null
}

Invoke-PaperBundleStep `
    -Name "core_tests" `
    -Command "dotnet" `
    -Arguments @("run", "--project", "tests\RomanAI.Core.Tests\RomanAI.Core.Tests.csproj", "--configfile", "NuGet.Config", "--no-build") | Out-Null

Invoke-PaperBundleStep `
    -Name "paper_ready_bundle" `
    -Command "dotnet" `
    -Arguments @(
        "run",
        "--project",
        "tools\RomanAI.Headless\RomanAI.Headless.csproj",
        "--configfile",
        "NuGet.Config",
        "--no-build",
        "--",
        "--paper-ready-bundle",
        "--paper-repetitions",
        ([string]$Repetitions),
        "--out",
        $OutputPath
    ) | Out-Null

Test-ExpectedFiles
Test-ManifestHashes
Test-ArtifactCommands
Test-ReadinessReport
Test-NegativeControls
Test-ClaimBoundaries

$result = [ordered]@{
    passed = ($errors.Count -eq 0)
    repoRoot = $repoRoot
    projectPath = $ProjectPath
    outputPath = $OutputPath
    repetitions = $Repetitions
    checks = $checks
    warnings = $warnings
    errors = $errors
}

$reportPath = Join-Path $OutputPath "paper_bundle_verification_report.json"
$json = $result | ConvertTo-Json -Depth 20
$json | Set-Content -LiteralPath $reportPath -Encoding UTF8
$json

if ($errors.Count -gt 0) {
    exit 1
}
