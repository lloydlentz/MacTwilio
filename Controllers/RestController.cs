using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ClientQuickstart.Controllers
{
    public class RestController : Controller
    {
        string ans = "";

        // Set our AccountSid and AuthToken
        string AccountSid = ConfigurationManager.AppSettings["TwilioAccountSid"]; 
        string AuthToken = ConfigurationManager.AppSettings["TwilioAuthToken"];

        string baseurl = "https://api.twilio.com/";
        string version = "0.0.1";

        RestClient _client;


        // GET: Rest
        public string Index()
        {
            _client = new RestClient();
            _client.UserAgent = "lentz-twilio-csharp/" + version + " (.NET " + Environment.Version.ToString() + ")";
            _client.Authenticator = new HttpBasicAuthenticator(AccountSid, AuthToken);
            _client.AddDefaultHeader("Accept-charset", "utf-8");

            _client.BaseUrl = new Uri(baseurl);
            _client.Timeout = 30500;

            RestRequest request = new RestRequest("2010-04-01/Accounts/" + AccountSid + "/Calls.json?PageSize=1000&StartDate", Method.GET);
            IRestResponse response = _client.Execute(request);
            var content = response.Content;

            MacTwilio.CallRequest callrequest = JsonConvert.DeserializeObject<MacTwilio.CallRequest>(content);
            int page = 1;
            while (string.IsNullOrWhiteSpace( callrequest.next_page_uri) == false)
            {
                foreach (MacTwilio.Call call in callrequest.calls)
                {
                    if (OracleRS.ExecuteScalar("select count(*) from twilio_call_log where sid = '" + call.sid + "'", "adv") == "0")
                    {
                        string sql = "insert into TWILIO_CALL_LOG( SID, PARENTCALLSID, DATECREATED, DATEUPDATED, ACCOUNTSID, CALL_TO,CALL_FROM,PHONENUMBERSID,STATUS,STARTTIME,ENDTIME,DURATION,PRICE,DIRECTION,ANSWEREDBY,FORWARDEDFROM,CALLERNAME,RESTEXCEPTION,URI) values ( '{0}' , '{1}' , to_date('{2}', 'mm/dd/yyyy HH12:MI:SS PM'), to_date('{3}', 'mm/dd/yyyy HH12:MI:SS PM') , '{4}' , '{5}' , '{6}' , '{7}' , '{8}' , to_date('{9}', 'mm/dd/yyyy HH12:MI:SS PM') , to_date('{10}', 'mm/dd/yyyy HH12:MI:SS PM') , {11} , {12} , '{13}' , '{14}' , '{15}' , '{16}' , '{17}' , '{18}' )";
                        sql = string.Format(sql,
                            call.sid,
                            call.parent_call_sid,
                            call.DateCreated.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            call.DateUpdated.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            call.account_sid,
                            call.to,
                            call.from,
                            call.phone_number_sid,
                            call.status,
                            call.StartTime.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            call.EndTime.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            call.duration ?? "0",
                            call.price ?? "0",
                            call.direction,
                            call.answered_by,
                            call.forwarded_from,
                            call.caller_name,
                            "na",
                            call.uri);
                        OracleRS.Execute(sql, "adv");
                        ans += call.sid + ",\n";
                    }

                }
                page++;
                request = new RestRequest(callrequest.next_page_uri, Method.GET);
                response = _client.Execute(request);
                callrequest = JsonConvert.DeserializeObject<MacTwilio.CallRequest>(response.Content);
            }


            //string url = string.Format( baseurl,sid + ":" + token) + sid + "/Calls.json";
            //return url;
            return "page(s)="+page.ToString() +  ans  + content;

        }
        public string Calls()
        {

            string url = string.Format(baseurl, AccountSid + ":" + AuthToken) + AccountSid + "/Calls.json";
            var json = new WebClient().DownloadString(url);
            return json;
        }
    }
}