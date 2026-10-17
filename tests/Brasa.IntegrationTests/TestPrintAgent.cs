using System.Net.Http.Headers;
using System.Net.Http.Json;
using Brasa.Application.Printing.Agents;
using Brasa.Application.Printing.Jobs;

namespace Brasa.IntegrationTests;

public sealed class TestPrintAgent
{
    public const string PaperAddress = "tcp://192.168.0.50:9100";

    private TestPrintAgent(HttpClient client, Guid agentId, string secret, PrinterResponse printer)
    {
        Client = client;
        AgentId = agentId;
        Secret = secret;
        Printer = printer;
    }

    public HttpClient Client { get; }

    public Guid AgentId { get; }

    public string Secret { get; }

    public PrinterResponse Printer { get; }

    public static HttpClient NewAgentClient(StoreApiFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(AgentProtocol.Header, AgentProtocol.Current.ToString());
        return client;
    }

    public static async Task<string> PairingCodeAsync(HttpClient owner) =>
        (await (await owner.PostAsync("/api/v1/admin/printing/pairing-codes", null)).ReadAsync<PairingCodeResponse>()).Code;

    public static async Task<TestPrintAgent> InstallAsync(StoreApiFactory factory)
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var client = NewAgentClient(factory);

        var paired = await (await client.PostAsJsonAsync("/api/v1/agent/pair", new
            {
                code = await PairingCodeAsync(owner),
                name = TestApi.Unique("Caixa"),
                agentVersion = "1.0.0",
            }))
            .ReadAsync<PairAgentResponse>();

        var printer = await (await owner.PostAsJsonAsync($"/api/v1/admin/printing/agents/{paired.AgentId}/printers", new
            {
                name = TestApi.Unique("Cozinha"),
                paperWidth = "Mm80",
                address = PaperAddress,
                isDefault = true,
            }))
            .ReadAsync<PrinterResponse>();

        var agent = new TestPrintAgent(client, paired.AgentId, paired.Secret, printer);
        await agent.SignInAsync();
        return agent;
    }

    public async Task SignInAsync()
    {
        var token = await (await Client.PostAsJsonAsync("/api/v1/agent/token", new { agentId = AgentId, secret = Secret })).ReadAsync<AgentTokenResponse>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public async Task<IReadOnlyList<LeasedPrintJob>> LeaseAsync() =>
        (await (await Client.PostAsJsonAsync("/api/v1/agent/jobs/lease", new { maxJobs = 20 })).ReadAsync<LeasedPrintJobsResponse>()).Jobs;

    public Task<HttpResponseMessage> ReportAsync(LeasedPrintJob job, string outcome, string? error = null) =>
        Client.PostAsJsonAsync($"/api/v1/agent/jobs/{job.JobId}/result", new { attemptId = job.AttemptId, outcome, error });
}
