using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Tests.LoadTests;

/// <summary>
/// Tests de montée en charge pour les endpoints authentifiés (POST, PUT, DELETE).
/// 
/// ⚠️ PRÉREQUIS :
///   1. L'API doit tourner localement sur http://localhost:5000
///   2. Configurer un JWT valide dans la constante BearerToken ci-dessous,
///      ou obtenir un token via l'endpoint /api/v1/auth/login avant les tests.
/// </summary>
public class AuthenticatedLoadTests
{
    private const string BaseUrl = "http://localhost:5000";
    // Remplacer par un token JWT valide avant de lancer les tests
    private const string BearerToken = "YOUR_JWT_TOKEN_HERE";
    private readonly ITestOutputHelper _output;

    public AuthenticatedLoadTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static HttpClient CreateAuthenticatedClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BearerToken);
        return client;
    }

    // ─────────────────────────────────────────────
    // 1. AUTH — POST /api/v1/auth/login
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_Login()
    {
        using var httpClient = new HttpClient();

        var loginPayload = JsonSerializer.Serialize(new
        {
            email = "testowner@example.com",
            password = "Test123!"
        });

        var scenario = Scenario.Create("login", async context =>
        {
            var request = Http.CreateRequest("POST", $"{BaseUrl}/api/v1/auth/login")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(loginPayload, Encoding.UTF8, "application/json"));
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_login")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"LOGIN — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms, P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");
    }

    // ─────────────────────────────────────────────
    // 2. BOOKINGS — POST /api/v1/bookings
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_CreateBooking()
    {
        using var httpClient = CreateAuthenticatedClient();
        var counter = 0;

        var scenario = Scenario.Create("create_booking", async context =>
        {
            var id = Interlocked.Increment(ref counter);
            var start = DateTime.UtcNow.AddDays(100 + id).ToString("yyyy-MM-dd");
            var end = DateTime.UtcNow.AddDays(107 + id).ToString("yyyy-MM-dd");

            var payload = JsonSerializer.Serialize(new
            {
                boatId = 1,
                startDate = start,
                endDate = end,
                message = $"Load test booking #{id}"
            });

            var request = Http.CreateRequest("POST", $"{BaseUrl}/api/v1/bookings")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(payload, Encoding.UTF8, "application/json"));

            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(20))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_create_booking")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"CREATE BOOKING — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
    }

    // ─────────────────────────────────────────────
    // 3. REVIEWS — POST /api/v1/review
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_CreateReview()
    {
        using var httpClient = CreateAuthenticatedClient();
        var counter = 0;

        var scenario = Scenario.Create("create_review", async context =>
        {
            var id = Interlocked.Increment(ref counter);
            var payload = JsonSerializer.Serialize(new
            {
                boatId = 1,
                rating = (id % 5) + 1,
                comment = $"Load test review #{id}",
                title = $"Review {id}"
            });

            var request = Http.CreateRequest("POST", $"{BaseUrl}/api/v1/review")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(payload, Encoding.UTF8, "application/json"));

            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(20))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_create_review")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"CREATE REVIEW — OK: {result.ScenarioStats[0].Ok.Request.Count}");
    }

    // ─────────────────────────────────────────────
    // 4. BOOKINGS — Workflow complet (browse → check → book)
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void LoadTest_BookingWorkflow()
    {
        using var httpClient = CreateAuthenticatedClient();
        var counter = 0;

        var scenario = Scenario.Create("booking_workflow", async context =>
        {
            // Étape 1: Browse boats
            var browseReq = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            await Http.Send(httpClient, browseReq);

            // Étape 2: Check availability
            var id = Interlocked.Increment(ref counter);
            var start = DateTime.UtcNow.AddDays(200 + id).ToString("yyyy-MM-dd");
            var end = DateTime.UtcNow.AddDays(207 + id).ToString("yyyy-MM-dd");

            var checkReq = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/availability/check?boatId=1&startDate={start}&endDate={end}");
            await Http.Send(httpClient, checkReq);

            // Étape 3: Create booking
            var payload = JsonSerializer.Serialize(new
            {
                boatId = 1,
                startDate = start,
                endDate = end,
                message = $"Workflow booking #{id}"
            });

            var bookReq = Http.CreateRequest("POST", $"{BaseUrl}/api/v1/bookings")
                .WithHeader("Content-Type", "application/json")
                .WithBody(new StringContent(payload, Encoding.UTF8, "application/json"));

            return await Http.Send(httpClient, bookReq);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(3))
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/load_booking_workflow")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
            .Run();

        _output.WriteLine($"WORKFLOW — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
    }

    // ─────────────────────────────────────────────
    // 5. CONCURRENT USERS — 50 utilisateurs simultanés
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void ConcurrencyTest_50Users()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("concurrent_users", async context =>
        {
            var endpoints = new[]
            {
                $"{BaseUrl}/api/v1/boats",
                $"{BaseUrl}/api/v1/destinations",
                $"{BaseUrl}/api/v1/review",
                $"{BaseUrl}/api/v1/boats/1",
                $"{BaseUrl}/api/v1/destinations/1"
            };

            var url = endpoints[Random.Shared.Next(endpoints.Length)];
            var request = Http.CreateRequest("GET", url);
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromSeconds(60))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/concurrent_50users")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md)
            .Run();

        _output.WriteLine($"50 CONCURRENT USERS — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  RPS: {result.ScenarioStats[0].Ok.Request.RPS}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms, P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");
    }

    // ─────────────────────────────────────────────
    // 6. SCALING — Montée de 10 à 200 users
    // ─────────────────────────────────────────────

    [Fact(Skip = "Requires running API server — run manually")]
    public void ScalingTest_RampUpUsers()
    {
        using var httpClient = new HttpClient();

        var scenario = Scenario.Create("ramp_up_users", async context =>
        {
            var request = Http.CreateRequest("GET", $"{BaseUrl}/api/v1/boats");
            return await Http.Send(httpClient, request);
        })
        .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: 10, during: TimeSpan.FromSeconds(10)),
            Simulation.KeepConstant(copies: 10, during: TimeSpan.FromSeconds(15)),
            Simulation.RampingConstant(copies: 50, during: TimeSpan.FromSeconds(10)),
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromSeconds(15)),
            Simulation.RampingConstant(copies: 100, during: TimeSpan.FromSeconds(10)),
            Simulation.KeepConstant(copies: 100, during: TimeSpan.FromSeconds(15)),
            Simulation.RampingConstant(copies: 200, during: TimeSpan.FromSeconds(10)),
            Simulation.KeepConstant(copies: 200, during: TimeSpan.FromSeconds(15))
        );

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("./reports/scaling_ramp_up")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
            .Run();

        _output.WriteLine($"RAMP UP — OK: {result.ScenarioStats[0].Ok.Request.Count}, Fail: {result.ScenarioStats[0].Fail.Request.Count}");
        _output.WriteLine($"  Max RPS: {result.ScenarioStats[0].Ok.Request.RPS}");
        _output.WriteLine($"  P50: {result.ScenarioStats[0].Ok.Latency.Percent50}ms");
        _output.WriteLine($"  P95: {result.ScenarioStats[0].Ok.Latency.Percent95}ms");
        _output.WriteLine($"  P99: {result.ScenarioStats[0].Ok.Latency.Percent99}ms");
    }
}
