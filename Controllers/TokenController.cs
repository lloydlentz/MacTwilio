using System.Configuration;
using System.Web.Mvc;
using Faker;
using Faker.Extensions;
using Twilio;

namespace ClientQuickstart.Controllers
{
  [AllowCrossSite]
  public class TokenController : Controller
  {
        // GET: /token
        public ActionResult Index()
        {
            // Load Twilio configuration from Web.config
            var accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            var authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            var appSid = ConfigurationManager.AppSettings["TwilioTwimlAppSid"];

            // Create a random identity for the client
            var id = Internet.UserName().AlphanumericOnly();

            // Create an Access Token generator
            var capability = new TwilioCapability(accountSid, authToken);
            capability.AllowClientOutgoing(appSid);
            capability.AllowClientIncoming(id);
            var token = capability.GenerateToken();

            return Json(new
            {
                id,
                token
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Setup(string id)
        {
            // Load Twilio configuration from Web.config
            var accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            var authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            var appSid = ConfigurationManager.AppSettings["TwilioTwimlAppSid"];

            if (string.IsNullOrWhiteSpace(id))
            {
                // Create a random identity for the client
                id = Internet.UserName().AlphanumericOnly();
            }

            // Create an Access Token generator
            var capability = new TwilioCapability(accountSid, authToken);
            capability.AllowClientOutgoing(appSid);
            capability.AllowClientIncoming(id);
            var token = capability.GenerateToken();

            return Json(new
            {
                id,
                token
            }, JsonRequestBehavior.AllowGet);
        }

    }
}
