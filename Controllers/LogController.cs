using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;

namespace ClientQuickstart.Controllers
{
    public class LogController : Controller
    {
        // GET: Log
        public ActionResult Index()
        {
            return View();
        }

        public String Update()
        {
            string ans = "";

            // Set our AccountSid and AuthToken
            string sid = ConfigurationManager.AppSettings["TwilioAccountSid"]; ;
            string token = ConfigurationManager.AppSettings["TwilioAuthToken"]; ;

            // Instantiate a new Twilio Rest Client
            var client = new TwilioRestClient(sid, token);

            // Get Recent Calls
            CallListRequest filter = new CallListRequest();
            filter.Count = 1000;
            filter.PageNumber = 2;
            filter.StartTime = DateTime.Now.AddDays(-2).Date;
            filter.StartTimeComparison = ComparisonType.GreaterThanOrEqualTo;
            var calls = client.ListCalls(filter);

            if (calls.RestException != null)
            {
                ans = string.Format("Error: {0}", calls.RestException.Message);
                return ans;                
            }

            foreach(Call call in calls.Calls)
            {
                if (OracleRS.ExecuteScalar("select count(*) from twilio_call_log where sid = '"+call.Sid+"'", "adv") == "0")
                {
                    string sql = "insert into TWILIO_CALL_LOG( SID, PARENTCALLSID, DATECREATED, DATEUPDATED, ACCOUNTSID, CALL_TO,CALL_FROM,PHONENUMBERSID,STATUS,STARTTIME,ENDTIME,DURATION,PRICE,DIRECTION,ANSWEREDBY,FORWARDEDFROM,CALLERNAME,RESTEXCEPTION,URI) values ( '{0}' , '{1}' , to_date('{2}', 'mm/dd/yyyy HH12:MI:SS PM'), to_date('{3}', 'mm/dd/yyyy HH12:MI:SS PM') , '{4}' , '{5}' , '{6}' , '{7}' , '{8}' , to_date('{9}', 'mm/dd/yyyy HH12:MI:SS PM') , to_date('{10}', 'mm/dd/yyyy HH12:MI:SS PM') , {11} , {12} , '{13}' , '{14}' , '{15}' , '{16}' , '{17}' , '{18}' )";
                    sql = string.Format(sql,
                        call.Sid,
                        call.ParentCallSid,
                        call.DateCreated.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        call.DateUpdated.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        call.AccountSid,
                        call.To,
                        call.From,
                        call.PhoneNumberSid,
                        call.Status,
                        call.StartTime.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        call.EndTime.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        call.Duration,
                        call.Price ?? 0,
                        call.Direction,
                        call.AnsweredBy,
                        call.ForwardedFrom,
                        call.CallerName,
                        call.RestException,
                        call.Uri.ToString());
                    OracleRS.Execute(sql, "adv");
                    ans += call.Sid + ",";
                }
            }

            return ans;
        }
    }
}