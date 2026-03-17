using System.Text.Json;

public class AzureDevOpsHttpClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public AzureDevOpsHttpClient(IConfiguration config)
    {
        _http = new HttpClient();
        string baseUrl = config["ado:baseUrl"];
        string org = config["ado:org"];
        string pat = config["ado:pat"];
        var authString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));

        _http.BaseAddress = new Uri($"{baseUrl}/{org}/");
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
        _http.DefaultRequestHeaders.Add("Accept", "application/json; api-version=7.0-preview");
    }

    public async Task<T> GetAsync<T>(string requestUri)
        where T : class
    {
        var response = await _http.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, options);
        return value;
    }
}
