using FindMe.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IpService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

//app.Use(async (context, next) =>
//{
 //   var ipService = context.RequestServices.GetRequiredService<IpService>();
 //   await ipService.FetchIpInfoAsync(context); // Fetch and store IP
 //   await next();
//});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public class IpService
{
    private readonly HttpClient _httpClient;
    private string _clientIp;
    private IpInfo _ipInfo;

    public IpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task FetchIpInfoAsync(HttpContext context)
    {
        //if (_ipInfo != null) return; // Prevent multiple API calls

        string clientIp = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(clientIp) || clientIp == "::1")
        {
            clientIp = await _httpClient.GetStringAsync("https://api64.ipify.org");
        }
        _clientIp = clientIp;

        _ipInfo = await _httpClient.GetFromJsonAsync<IpInfo>($"https://ipinfo.io/{clientIp}/json");
        LogIpInfo(_clientIp, _ipInfo);
    }

    public async Task<IpInfo> GetIpInfoAsync(HttpContext context)
    {
        string clientIp = context.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(clientIp) || clientIp == "::1")
        {
            // Use external service to get public IP
            var ipResponse = await _httpClient.GetFromJsonAsync<IpInfo>("https://ipinfo.io/json");
            clientIp = ipResponse?.Ip;
        }

        var ipInfo = await _httpClient.GetFromJsonAsync<IpInfo>($"https://ipinfo.io/{clientIp}/json");
        LogIpInfo(ipInfo);
        return ipInfo;
    }

    public IpInfo GetIpInfo() => _ipInfo;

    private void LogIpInfo(string clientIp, IpInfo ipInfo)
    {
        string logFilePath = "ip_logs.txt";
        string logEntry = $"{DateTime.Now}: IP={clientIp}, City={ipInfo?.City}, Region={ipInfo?.Region}, Country={ipInfo?.Country}, Loc={ipInfo?.Loc}, Org={ipInfo?.Org}\n";
        File.AppendAllText(logFilePath, logEntry);
    }

    private void LogIpInfo(IpInfo ipInfo)
    {
        string logFilePath = "ip_logs.txt";
        string logEntry = $"{DateTime.Now}: IP={ipInfo?.Ip}, City={ipInfo?.City}, Region={ipInfo?.Region}, Country={ipInfo?.Country}, Loc={ipInfo?.Loc}, Org={ipInfo?.Org}\n";
        File.AppendAllText(logFilePath, logEntry);
    }
}

public class IpInfo
{
    public string Ip { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Loc { get; set; } = string.Empty;
    public string Org { get; set; } = string.Empty;
}

