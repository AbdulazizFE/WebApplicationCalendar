using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using System.Net;
using System.Threading.Tasks;
using System;
using WebApplicationCalendar.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;

using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using AngleSharp.Io;
using Microsoft.Extensions.Logging;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    public TokenService(IConfiguration configuration, IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _memoryCache = memoryCache;
    }



    public MyToken RefreshToken()
    {
        var refreshToken = _memoryCache.Get<string>("RefreshToken");
        var restClient = new RestClient();
        var restRequest = new RestRequest();
        restRequest.AddParameter("client_id", _configuration["appSettings:client_id"]);
        restRequest.AddParameter("grant_type", "refresh_token");
        restRequest.AddParameter("refresh_token", refreshToken);
        restRequest.AddParameter("scope", _configuration["appSettings:scopes"]);
        restRequest.AddParameter("redirect_uri", _configuration["appSettings:redirect_url"]);
        restRequest.AddParameter("client_secret", _configuration["appSettings:client_secret"]);

        restClient.BaseUrl = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/token");
        var response =  restClient.Post(restRequest);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var token = Newtonsoft.Json.JsonConvert.DeserializeObject<MyToken>(response.Content);
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
            var refreshTokenTime = new MemoryCacheEntryOptions();
            _memoryCache.Set("Token", token.access_token, cacheEntryOptions);
            _memoryCache.Set("RefreshToken", token.refresh_token, refreshTokenTime);
            return token;
        }

        throw new Exception($"Failed to refresh token. Status code: {response.StatusCode}. Content: {response.Content}");
    }


   
}
