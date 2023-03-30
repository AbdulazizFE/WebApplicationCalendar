using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplicationCalendar.Models;
using System.Net.Http.Formatting;
using RestSharp;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace WebApplicationCalendar.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        //private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        //private readonly ITokenService _tokenService;
        //private readonly AuthController _authController;
        //private readonly CalendarEvent _calendarEvent;
        public HomeController(IMemoryCache memoryCache)
        {
            //_logger = logger;
            //_configuration = config;
            _memoryCache = memoryCache;
            //_tokenService = tokenService;
            //_authController = authController;
        }

        public ActionResult Index()
        {
            var token = _memoryCache.Get("Token");
            IEnumerable<CalendarEvent> calendarEvents = Enumerable.Empty<CalendarEvent>();
            if (token == null)
            {
                return RedirectToAction("OauthRedirect", "Auth");
             
                //_tokenService.RefreshToken();
                //token = _memoryCache.Get("Token");
            }
      
            try
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();

                // Get the current date and time
                DateTimeOffset now = DateTimeOffset.UtcNow;
                // Calculate the end date and time for the query
                DateTimeOffset endOfWeek = now.AddDays(7);
                // Define the query parameters to retrieve events within the next 7 days
                restRequest.AddParameter("startDateTime", now.ToString("o"));
                restRequest.AddParameter("endDateTime", endOfWeek.ToString("o"));
                restRequest.AddHeader("Authorization", "Bearer " + token);
                //remove the html tag from the description
                restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");
                //restRequest.AddHeader("Prefer", "outlook.timezone=\"Sweden Standard Time\"");
                restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/me/calendar/calendarView");
                var response = restClient.Get(restRequest);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject eventsList = JObject.Parse(response.Content);
                    string nextWeekUrl = eventsList["@odata.nextLink"]?.ToString();
                    while (!string.IsNullOrEmpty(nextWeekUrl))
                    {
                        // Make a request to the next URL to get the next set of events
                        restRequest = new RestRequest(Method.GET);
                        restRequest.Resource = nextWeekUrl;
                        restRequest.AddHeader("Authorization", "Bearer " + token);
                        restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");
                        response = restClient.Execute(restRequest);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            eventsList = JObject.Parse(response.Content);
                            IEnumerable<CalendarEvent> nextEvents = eventsList["value"].ToObject<IEnumerable<CalendarEvent>>();
                            calendarEvents = calendarEvents.Concat(nextEvents);
                            nextWeekUrl = eventsList["@odata.nextLink"]?.ToString();
                        }
                    }
                    calendarEvents = eventsList["value"].ToObject<IEnumerable<CalendarEvent>>();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
            return View(calendarEvents);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


