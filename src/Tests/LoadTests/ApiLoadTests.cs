using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace Tests.LoadTests;

/// <summary>
/// Tests de montée en charge (load tests) pour l'API SailingLoc.
/// 
/// ⚠️ PRÉREQUIS : L'API doit tourner localement sur http://localhost:5000
///    (ou modifier BaseUrl ci-dessous).
///    Lancez l'API dans un terminal séparé avant d'exécuter ces tests :
///    dotnet run --project src/Api
/// 
/// Les rapports HTML sont générés dans le dossier ./reports/
/// </summary>
public class ApiLoadTests
{
    private const string BaseUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;

    public ApiLoadTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ─────────────────────────────────────────────
    // 1. BOATS — GET /api/v1/boats (endpoint public)
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_GetBoats_Sustained()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("get_boats", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_boats")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
            .Run();

        _output.WriteLine($"GET /boats — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  Latency P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms, P95: {result.ScenarioStats[0].Ok.Latency.Percent95}ms, P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");

        Assert.True(result.ScenarioStats[0].Fail.Request.Count == 0, "Aucune requête ne devrait échouer sous charge normale");
    }

    // ─────────────────────────────────────────────
    // 2. BOATS — GET /api/v1/boats/{id}
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_GetBoatById_Sustained()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("get_boat_by_id", async context =>
        {
            var boatId = (context.ScenarioInfo.ThreadNumber % 5) + 1;
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats/{boatId}");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_boat_by_id")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"GET /boats/{{id}} — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms, P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");
    }

    // ─────────────────────────────────────────────
    // 3. DESTINATIONS — GET /api/v1/destinations
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_GetDestinations_Sustained()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("get_destinations", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/destinations");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 80, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_destinations")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"GET /destinations — OK: {result.ScenarioStats[0].Ok.Request.Count}");
    }

    // ─────────────────────────────────────────────
    // 4. REVIEWS — GET /api/v1/review
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_GetReviews_Sustained()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("get_reviews", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/review");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 60, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_reviews")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"GET /review — OK: {result.ScenarioStats[0].Ok.Request.Count}");
    }

    // ─────────────────────────────────────────────
    // 5. AVAILABILITY — GET /api/v1/availability/check
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_CheckAvailability_Sustained()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("check_availability", async context =>
        {
            var start = DateTime.UtcNow.AddDays(50).ToString("yyyy-MM-dd");
            var end = DateTime.UtcNow.AddDays(57).ToString("yyyy-MM-dd");
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/availability/check?boatId=1&startDate={start}&endDate={end}");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 40, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_availability")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"AVAILABILITY — OK: {result.ScenarioStats[0].Ok.Request.Count}");
    }

    // ─────────────────────────────────────────────
    // 6. SPIKE TEST — Pic soudain de trafic
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void SpikeTest_GetBoats()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("spike_boats", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10)),
            Simulation.Inject(rate: 200, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15)),
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/spike_boats")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"SPIKE — Total OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms, P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");
    }

    // ─────────────────────────────────────────────
    // 7. STRESS TEST — Montée progressive jusqu'à saturation
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void StressTest_GradualIncrease()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("stress_gradual", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            var response = await Http.Send(httpClient, request);
            return response;
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15)),
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15)),
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15)),
            Simulation.Inject(rate: 200, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(15)),
            Simulation.Inject(rate: 500, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/stress_gradual")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
            .Run();

        _output.WriteLine($"STRESS — Total OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms");
        _output.WriteLine($"  P95: {result.ScenarioStats[0].Ok.Latency.Percent95}ms");
        _output.WriteLine($"  P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");
    }

    // ─────────────────────────────────────────────
    // 8. MIXED SCENARIO — Simulation réaliste multi-endpoint
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void MixedScenario_RealisticTraffic()
    {
        using var httpClient = new HttpClient();

        var browsing = Scenario.Create("browse_boats", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var details = Scenario.Create("view_boat_detail", async context =>
        {
            var boatId = (context.ScenarioInfo.ThreadNumber % 3) + 1;
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats/{boatId}");
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var destinations = Scenario.Create("browse_destinations", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/destinations");
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var reviews = Scenario.Create("browse_reviews", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/review/recent?limit=10");
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var result = NBomberRunner
            .RegisterScenarios(browsing, details, destinations, reviews)
            .WithReportFolder("./reports/mixed_realistic")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
            .Run();

        foreach (var stat in result.ScenarioStats)
        {
            _output.WriteLine($"{stat.ScenarioName}: OK={stat.Ok.Request.Count}, Fail={stat.Fail.Request.Count}, P50={stat.Ok.Latency.Percent50}ms, P99={stat.Ok.Latency.Percent99}ms");
        }
    }

    // ─────────────────────────────────────────────
    // 9. ENDURANCE TEST — Test longue durée (5 min)
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually. Long test (~5 min).")]
    public void EnduranceTest_5Minutes()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("endurance_boats", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(10))
        .WithLoadSimulations(
            Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(5))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/endurance_5min")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"ENDURANCE 5min — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms, P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");

        Assert.True(result.ScenarioStats[0].Fail.Request.Count == 0, "Aucune requête ne devrait échouer en endurance");
    }
}
