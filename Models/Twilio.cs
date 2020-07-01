using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MacTwilio
{
    public class Twilio
    {
    }
    public class CallRequest
    {
        public string first_page_uri { get; set; }
        public int end { get; set; }
        public List<Call> calls { get; set; }
        public object previous_page_uri { get; set; }
        public string uri { get; set; }
        public int page_size { get; set; }
        public int start { get; set; }
        public string next_page_uri { get; set; }
        public int page { get; set; }

    }
    public class Call
    {

        public string sid { get; set; }
        public DateTime DateCreated { get; set; }
        public string date_created {
            get { return DateCreated.ToLongDateString(); }
            set {
                DateTime dt = DateTime.Now.AddYears(-10);
                DateTime.TryParse(value, out dt);
                DateCreated = dt;
            }
        }
        public DateTime DateUpdated { get; set; }
        public string date_updated
        {
            get { return DateUpdated.ToLongDateString(); }
            set
            {
                DateTime dt = DateTime.Now.AddYears(-10);
                DateTime.TryParse(value, out dt);
                DateUpdated = dt;
            }
        }
        public string parent_call_sid { get; set; }
        public string account_sid { get; set; }
        public string to { get; set; }
        public string to_formatted { get; set; }
        public string from { get; set; }
        public string from_formatted { get; set; }
        public string phone_number_sid { get; set; }
        public string status { get; set; }
        public DateTime StartTime { get; set; }
        public string start_time
        {
            get { return StartTime.ToLongDateString(); }
            set
            {
                DateTime dt = DateTime.Now.AddYears(-10);
                DateTime.TryParse(value, out dt);
                StartTime = dt;
            }
        }
        public DateTime EndTime { get; set; }
        public string end_time
        {
            get { return EndTime.ToLongDateString(); }
            set
            {
                DateTime dt = DateTime.Now.AddYears(-10);
                DateTime.TryParse(value, out dt);
                EndTime = dt;
            }
        }
        public string duration { get; set; }
        public string price { get; set; }
        public string price_unit { get; set; }
        public string direction { get; set; }
        public object answered_by { get; set; }
        public object annotation { get; set; }
        public string api_version { get; set; }
        public object forwarded_from { get; set; }
        public object group_sid { get; set; }
        public object caller_name { get; set; }
        public string uri { get; set; }
        public SubresourceUris subresource_uris { get; set; }
    }
    public class SubresourceUris
    {
        public string notifications { get; set; }
        public string recordings { get; set; }
    }
}