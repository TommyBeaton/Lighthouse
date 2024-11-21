using System.Timers;
using Kurrent.Interfaces;
using Kurrent.Interfaces.Polling;
using Kurrent.Models.Data;
using Kurrent.Utils;

namespace Kurrent.Implementation.Polling.Pollers;

public abstract class BasePoller : IPoller
{
    private readonly ISubscriptionHandler _subscriptionHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    
    protected abstract string Type { get; }
    private System.Timers.Timer _timer;

    protected PollerConfig? Config;
    
    private readonly Dictionary<string, string> _latestTags = new();
    private ElapsedEventHandler _handler;

    protected BasePoller(
        ISubscriptionHandler subscriptionHandler,
        IHttpClientFactory httpClientFactory,
        ILogger logger)
    {
        _subscriptionHandler = subscriptionHandler;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void Start(PollerConfig config, CancellationToken token)
    {
        if (config.Type != Type)
        {
            _logger.LogWarning("Failed to start {type} poller. Poller type is not {type}. Poller config: {poller}",
                Type,
                Type,
                config);
        }

        _handler = (sender, e) => 
        {
            Task.Run(async () => await CheckForUpdates(token), token).Wait(token);
        };

        Config = config;
        _timer = new System.Timers.Timer(Config.IntervalInSeconds * 1000);
        _timer.Elapsed += _handler;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _timer.Elapsed -= _handler;
        _timer.Dispose();
    }

    private async Task CheckForUpdates(CancellationToken token)
    {
        foreach (var imageName in Config.Images)
        {
            if(token.IsCancellationRequested) //Check if we should stop
                Stop();
            
            _logger.LogInformation("Checking for {image} updates", imageName);
            if(!_latestTags.TryGetValue(imageName, out string latestKnownTag))
            {
                latestKnownTag = String.Empty;
                _latestTags.Add(imageName, latestKnownTag);
            }
        
            using var client = _httpClientFactory.CreateClient();
            using var httpResponse = await MakeHttpRequest(client, imageName);

            if (httpResponse == null)
                continue;

            string jsonResponse = await httpResponse.Content.ReadAsStringAsync(token);

            var latestTag = ExtractLatestTag(jsonResponse);

            if(string.IsNullOrEmpty(latestTag) || latestKnownTag == latestTag)
                continue;

            _latestTags[imageName] = latestTag;

            _logger.LogInformation("Found new tag {tag} for {image}", latestTag, imageName);
            
            var image = new Image(Config.Url, imageName, latestTag);
            await _subscriptionHandler.UpdateAsync(Config.EventName, image);
        }
    }

    protected abstract Task<HttpResponseMessage?> MakeHttpRequest(HttpClient client, string image);
    protected abstract string ExtractLatestTag(string httpResponse);
}