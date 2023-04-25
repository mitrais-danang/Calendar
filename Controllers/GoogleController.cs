using BuzCalendarV2.Models;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Calendar.v3.Data;
using System.Globalization;
using Calendar = Google.Apis.Calendar.v3.Data.Calendar;

namespace BuzCalendarV2.Controllers
{
    public class GoogleController : Controller
    {
        private readonly IConfiguration _config;
        public GoogleController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult GoogleCallBack(string code)
        {
            return Ok(code);
        }

        public IActionResult BuildConsentURL()
        {
            string[] scopes = { "https://www.googleapis.com/auth/calendar" };
            string url = GoogleHelper.BuildConsentURL(
                _config.GetValue<string>("ClientID"),
                scopes,
                "code",
                "offline",
                "true",
                "consent",
                 _config.GetValue<string>("Callbackurl")
                );
            return Ok(url);
        }

        public async Task<IActionResult> GetTokenAsync(string code)
        {
            GoogleAuthResponse tokenResponse = await GoogleHelper.ExchangeAuthorizationCode(code,
                _config.GetValue<string>("ClientID"),
                _config.GetValue<string>("Secret"),
                _config.GetValue<string>("Callbackurl")
                );
            return Ok(tokenResponse);
        }

        public async Task<IActionResult> RefreshTokenAsync(string token)
        {
            GoogleAuthResponse tokenResponse = await GoogleHelper.ExchangeRefreshToken(token,
               _config.GetValue<string>("ClientID"),
               _config.GetValue<string>("Secret")
                );
            return Ok(tokenResponse);
        }


        public async Task<IActionResult> GetCalendarList(string accessToken, string refreshToken)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _config.GetValue<string>("ClientID"),
                    ClientSecret = _config.GetValue<string>("Secret")
                },
                Scopes = new[] { CalendarService.Scope.Calendar }
            }); ;

            var credential = new UserCredential(flow, "user_id", new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            var events = await service.CalendarList.List().ExecuteAsync();

            return Ok(events);
        }

        public async Task<IActionResult> GetCalendar(string accessToken, string refreshToken, string calendarID)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _config.GetValue<string>("ClientID"),
                    ClientSecret = _config.GetValue<string>("Secret")
                },
                Scopes = new[] { CalendarService.Scope.Calendar }
            });

            var credential = new UserCredential(flow, "user_id", new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            var events = await service.CalendarList.Get(calendarID).ExecuteAsync();

            return Ok(events);
        }

        public async Task<IActionResult> GetCalendarEvent(string accessToken, string refreshToken, string calendarID)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _config.GetValue<string>("ClientID"),
                    ClientSecret = _config.GetValue<string>("Secret")
                },
                Scopes = new[] { CalendarService.Scope.Calendar }
            });

            var credential = new UserCredential(flow, "user_id", new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            var events = await service.Events.List(calendarID).ExecuteAsync();

            return Ok(events);
        }

        public async Task<IActionResult> AddCalendar(string accessToken, string refreshToken, string calendarID)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _config.GetValue<string>("ClientID"),
                    ClientSecret = _config.GetValue<string>("Secret")
                },
                Scopes = new[] { CalendarService.Scope.Calendar }
            });

            var credential = new UserCredential(flow, "user_id", new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            Calendar calendar = new Calendar();

            calendar.Summary = "Test Buzz Calendar Summary";
            calendar.TimeZone = "Australia/Melbourne";

            var res = await service.Calendars.Insert(calendar).ExecuteAsync();

            return Ok(res);

        }

        public async Task<IActionResult> AddCalendarEvent(string accessToken, string refreshToken, string calendarID)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _config.GetValue<string>("ClientID"),
                    ClientSecret = _config.GetValue<string>("Secret")
                },
                Scopes = new[] { CalendarService.Scope.Calendar }
            });

            var credential = new UserCredential(flow, "user_id", new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            Event calendarEvent = new Event();
            calendarEvent.Summary = "Test Buzz Calendar Summary";
            calendarEvent.Location = "Test Buzz Calendar Location";
            calendarEvent.Description = "Test Buzz Calendar Description";

            DateTime startDateTime = DateTime.Parse("2023-05-28T09:00:00-07:00");
            EventDateTime start = new EventDateTime();
            start.DateTime = startDateTime;
            start.TimeZone = "Australia/Melbourne";
            calendarEvent.Start = start;

            DateTime endDateTime = DateTime.Parse("2023-05-28T09:00:00-08:00");
            EventDateTime end = new EventDateTime();
            end.DateTime = endDateTime;
            end.TimeZone = "Australia/Melbourne";
            calendarEvent.End = end;

            List<string> recurrence = new List<string> { "RRULE:FREQ=DAILY;COUNT=2" };
            calendarEvent.Recurrence = recurrence;

            EventAttendee[] attendees = new EventAttendee[] {
                new EventAttendee
                {
                    Email = "kusumayudha32@gmail.com"
                }
            };

            calendarEvent.Attendees = attendees;

            EventReminder[] reminderOverrides = new EventReminder[] {
                new EventReminder
                {
                    Method = "email",
                    Minutes = 24 * 60
                },
                new EventReminder
                {
                    Method = "popup",
                    Minutes = 10
                }
            };

            Event.RemindersData reminders = new Event.RemindersData
            {
                UseDefault = false,
                Overrides = reminderOverrides
            };

            calendarEvent.Reminders = reminders;
            var res = await service.Events.Insert(calendarEvent,calendarID).ExecuteAsync();

            return Ok(res.HtmlLink);

        }


    }
}
