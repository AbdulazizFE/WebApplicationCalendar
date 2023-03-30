using System;


namespace WebApplicationCalendar.Models
{
    public class CalendarEvent
    {
        public CalendarEvent()
        {
            this.body = new Body()
            {
                ContentType = "html"
            };


            this.Start = new EventDateTime()
            {
                TimeZone = "Europe/Stockholm"
            };

            this.End = new EventDateTime()
            {
                TimeZone = "Europe/Stockholm"
            };

            this.location = new Location()
            {

            };

        }

        public string Subject { get; set; }
        public Body body { get; set; }
        public EventDateTime Start { get; set; }
        public EventDateTime End { get; set; }
        public Location location { get; set; }

        public class Body
        {
            public string ContentType { get; set; }
            public string Content { get; set; }
        }

        public class EventDateTime
        {
            public DateTime DateTime { get; set; }
            public string TimeZone { get; set; }
        }

        public class Location{

            public string DisplayName { get; set; } 
        }


    }
}
