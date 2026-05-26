using RomanAI;
using RomanAI.Headless;

RomanHeadlessRunOptions options = RomanHeadlessRunOptions.Parse(args);

if (options.paperReadyBundle)
{
    RomanPaperReadyBundleResult bundle = RomanPaperExperimentRunner.CreatePaperReadyBundle(
        options.paperRepetitions,
        options.paperSeedStart,
        options.paperManifestPath,
        options.paperBroaderManifestPath);

    Console.WriteLine("RomanAI.Headless paper ready bundle");
    Console.Write(RomanPaperExperimentRunner.ToPaperResultsMarkdown(bundle));
    Console.WriteLine();
    Console.WriteLine("RomanAI.Headless paper readiness");
    Console.Write(RomanPaperExperimentRunner.ToReadinessCsv(bundle));

    if (options.writeFiles)
    {
        RomanPaperExperimentRunner.WritePaperReadyBundle(bundle, options.outputDirectory);
        Console.WriteLine("wrote_paper_ready_bundle_outputs=" + options.outputDirectory);
    }

    if (options.emitJson)
    {
        Console.WriteLine();
        Console.WriteLine("RomanAI.Headless paper ready bundle JSON");
        Console.WriteLine(RomanPaperExperimentRunner.ToPaperReadyBundleJson(bundle));
    }

    return bundle.passed ? 0 : 1;
}

if (options.paperExperiment)
{
    RomanPaperExperimentResult experiment = RomanPaperExperimentRunner.Run(
        options.paperRepetitions,
        options.paperSeedStart,
        options.paperManifestPath,
        options.paperNegativeControls);

    Console.WriteLine("RomanAI.Headless paper experiment");
    Console.Write(RomanPaperExperimentRunner.ToSummaryCsv(experiment));
    Console.WriteLine();
    Console.WriteLine("RomanAI.Headless paper experiment runs");
    Console.Write(RomanPaperExperimentRunner.ToRowsCsv(experiment));
    Console.WriteLine();
    Console.WriteLine("RomanAI.Headless paper experiment negative controls");
    Console.Write(RomanPaperExperimentRunner.ToNegativeControlsCsv(experiment));
    Console.WriteLine();
    Console.WriteLine("RomanAI.Headless paper experiment failure taxonomy");
    Console.Write(RomanPaperExperimentRunner.ToFailureTaxonomyCsv(experiment));

    if (options.writeFiles)
    {
        RomanPaperExperimentRunner.WriteResultFiles(experiment, options.outputDirectory);
        Console.WriteLine("wrote_paper_experiment_outputs=" + options.outputDirectory);
    }

    if (options.emitJson)
    {
        Console.WriteLine();
        Console.WriteLine("RomanAI.Headless paper experiment JSON");
        Console.WriteLine(RomanPaperExperimentRunner.ToJson(experiment));
    }

    return experiment.passed ? 0 : 1;
}

RomanValidationScenarioManifest validationManifest = string.IsNullOrWhiteSpace(options.validationManifestPath)
    ? RomanValidationScenarioSet.LoadDefaultManifest()
    : RomanValidationScenarioSet.LoadManifest(options.validationManifestPath);
RomanHeadlessBatchResult batch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios(validationManifest, options.tickOverride));
RomanValidationScenarioReport validation = RomanValidationScenarioSet.Evaluate(
    batch,
    validationManifest,
    string.IsNullOrWhiteSpace(options.validationManifestPath)
        ? RomanValidationScenarioSet.ResolveDefaultManifestPath()
        : options.validationManifestPath);

Console.WriteLine("RomanAI.Headless scenario batch");
Console.Write(RomanHeadlessScenarioRunner.ToSummaryCsv(batch));
Console.WriteLine();
Console.WriteLine("RomanAI.Headless tick trace");
Console.Write(RomanHeadlessScenarioRunner.ToTickCsv(batch));
Console.WriteLine();
Console.WriteLine("RomanAI.Headless validation scenarios");
Console.Write(RomanValidationScenarioSet.ToCsv(validation));

if (options.writeFiles)
{
    RomanHeadlessScenarioRunner.WriteResultFiles(batch, options.outputDirectory);
    RomanValidationScenarioSet.WriteReportFiles(validation, validationManifest, options.outputDirectory);
    Console.WriteLine("wrote_headless_outputs=" + options.outputDirectory);
}

if (options.emitJson)
{
    Console.WriteLine();
    Console.WriteLine("RomanAI.Headless batch JSON");
    Console.WriteLine(RomanHeadlessScenarioRunner.ToJson(batch));
    Console.WriteLine("RomanAI.Headless validation JSON");
    Console.WriteLine(RomanValidationScenarioSet.ToJson(validation));
}

RomanHumanPerformanceModifierProfile profile = RomanHumanPerformanceStateModel.CreateSyntheticStimulantLikeProfile();
RomanCombatStress baseline = new RomanCombatStress
{
    fatigue = 0.7f,
    confidence = 0.45f,
    morale = 0.6f
};

Console.WriteLine("RomanAI.Headless synthetic human-performance probe");
Console.WriteLine("time_s,fatigue,confidence,hesitation,aggression");

foreach (float seconds in new[] { 0f, 30f, 60f, 120f, 300f })
{
    RomanBehaviorStateDelta delta = RomanHumanPerformanceStateModel.Evaluate(profile, seconds);
    RomanCombatStress adjusted = RomanHumanPerformanceStateModel.ApplyToStress(baseline, delta);
    Console.WriteLine(
        $"{seconds:0},{adjusted.fatigue:0.000},{adjusted.confidence:0.000},{delta.hesitation:0.000},{delta.aggression:0.000}");
}

return batch.passed && validation.passed ? 0 : 1;
