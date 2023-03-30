using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplicationCalendar.Models;

namespace WebApplicationCalendar.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IMemoryCache _memoryCache;

        public AuthController(
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
        }
        public ActionResult OauthRedirect()
        {
            var redirectUrl = "/";
            try
            {
                redirectUrl = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?" +
                    "&scope=" + _configuration.GetSection("appSettings:scopes").Value +
                    "&response_type=code" +
                    "&response_mode=query" +
                    "&state=themessydeveloper" +
                    "&login_hint=Abdulaziz@6wd7fd.onmicrosoft.com" +
                    "&redirect_uri=" + _configuration.GetSection("appSettings:redirect_url").Value +
                    "&client_id=" + _configuration.GetSection("appSettings:client_id").Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Redirect(redirectUrl);
        }

        public ActionResult Callback(string code, string state, string error)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();
                restRequest.AddParameter("client_id", _configuration.GetSection("appSettings:client_id").Value);
                restRequest.AddParameter("scope", _configuration.GetSection("appSettings:scopes").Value);
                restRequest.AddParameter("redirect_uri", _configuration.GetSection("appSettings:redirect_url").Value);
                restRequest.AddParameter("client_secret", _configuration.GetSection("appSettings:client_secret").Value);
                restRequest.AddParameter("code", code);
                restRequest.AddParameter("login_hint", "Abdulaziz@6wd7fd.onmicrosoft.com");
                restRequest.AddParameter("grant_type", "authorization_code");

                restClient.BaseUrl = new Uri($"https://login.microsoftonline.com/common/oauth2/v2.0/token");
                var response = restClient.Post(restRequest);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var token = JsonConvert.DeserializeObject<MyToken>(response.Content);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(double.Parse("1")));
                    var refreshTokenTime = new MemoryCacheEntryOptions();
                    _memoryCache.Set("Token", token.access_token, cacheEntryOptions);
                    _memoryCache.Set("RefreshToken", token.refresh_token, refreshTokenTime);

                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Error");
        }

    }
}
