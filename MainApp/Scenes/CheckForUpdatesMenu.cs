using System;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;
using Semver;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace MainApp.Scenes;

public sealed class CheckForUpdatesMenu : Scene
{
    public CheckForUpdatesMenu(AppContext ctx) : base(ctx)
    {
        _currentVersion = SemVersion.Parse("2.0.0", SemVersionStyles.Strict);
        _strings = ctx.LanguageManager.Strings.CheckForUpdatesMenuScene;
        _logger = ctx.Logger;
        _appState = ctx.AppState;
        _owner = "AlexDeFoc";
        _repo = "GameWatchCon";

        // Build the message handler chain with caching
        var handler = new EtagCachingHandler(new HttpClientHandler());

        // Create the connection with our cached HTTP adapter
        var connection = new Connection(
            new Octokit.ProductHeaderValue("GameWatchCon-LatestVersionChecker"),
            new HttpClientAdapter(() => handler)
        );
        _client = new GitHubClient(connection);
    }

    public override void Run(SceneManager manager)
    {
        CheckForUpdates();
        RequestInput();
        manager.ReturnToPreviousScene();
    }

    private void CheckForUpdates()
    {
        Console.Clear();
        _logger.WriteCached();

        var latestTag = GetLatestReleaseTagAsync().GetAwaiter().GetResult();
        if (latestTag is null)
        {
            _logger.WriteLine(Logger.Label.Info, _strings.CurrentVersion(_currentVersion));
        }
        else
        {
            var latestVersionFound = SemVersion.Parse(latestTag, SemVersionStyles.Strict);

            if (_currentVersion.ComparePrecedenceTo(latestVersionFound) < 0)
            {
                _logger.WriteLine(Logger.Label.Success, _strings.NewVersionFoundMsg);
                _logger.WriteLine(Logger.Label.Info, _strings.CurrentVersion(_currentVersion));
                _logger.WriteLine(Logger.Label.Info, _strings.LatestVersionFound(latestVersionFound));
                _logger.WriteLine(Logger.Label.Info, _strings.NoticeOnUpdateOptionAvailableMsg);
                _appState.ToggleUpdateAvailableStatus();
            }
            else
            {
                _logger.WriteLine(Logger.Label.Success, _strings.NoNewVersionFoundMsg);
                _logger.WriteLine(Logger.Label.Info, _strings.CurrentVersion(_currentVersion));
                _logger.WriteLine(Logger.Label.Info, _strings.LatestVersionFound(latestVersionFound));
            }
        }
    }

    private async Task<string?> GetLatestReleaseTagAsync()
    {
        try
        {
            var latestRelease = await _client.Repository.Release.GetLatest(_owner, _repo);
            return latestRelease?.TagName;
        }
        catch (NotFoundException)
        {
            _logger.WriteLineToCache(Logger.Label.Error, _strings.NoReleasesFoundMsg);
            return null;
        }
        catch (RateLimitExceededException e)
        {
            _logger.WriteLineToCache(Logger.Label.Error, _strings.RateLimitExceeded(e.Reset.UtcDateTime));
            return null;
        }
    }

    private void RequestInput()
    {
        _logger.WriteLine(Logger.Label.Request, _strings.RequestInputMsg);
        Console.ReadKey();
    }

    // Aliases
    private readonly LanguageManager.ICheckForUpdatesMenuSceneStrings _strings;
    private readonly Logger _logger;
    private readonly AppState _appState;

    // Private variables
    private readonly SemVersion _currentVersion;
    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repo;

    // Private structures
    private sealed class EtagCachingHandler : DelegatingHandler
    {
        private readonly ConcurrentDictionary<string, CachedResponse> _cache = new();

        public EtagCachingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string? cacheKey = null;
            if (request.Method == HttpMethod.Get && request.RequestUri != null)
            {
                cacheKey = request.RequestUri.ToString();
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    request.Headers.IfNoneMatch.Clear();
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cached.Etag));
                }
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (cacheKey != null && response.StatusCode == HttpStatusCode.NotModified)
            {
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    response.Dispose(); // Dispose the 304 response
                    var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(cached.Content),
                        RequestMessage = request
                    };
                    foreach (var header in cached.Headers)
                    {
                        cachedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    return cachedResponse;
                }
            }

            if (cacheKey != null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var etag = response.Headers.ETag?.Tag;
                if (!string.IsNullOrEmpty(etag))
                {
                    _cache[cacheKey] = new CachedResponse
                    {
                        Etag = etag,
                        Content = content,
                        Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value)
                    };
                }
            }

            return response;
        }

        private sealed class CachedResponse
        {
            public required string Etag { get; init; }
            public required string Content { get; init; }
            public Dictionary<string, IEnumerable<string>> Headers { get; init; } = new();
        }
    }
}