using System;
using System.Web;
using GTPool;

namespace NewsSearch.Models
{
    public class SessionLog
    {
        private bool _isLoaded;

        public SessionLog(HttpContext httpContext)
        {
            LoadSessionLog(httpContext);
        }

        private void LoadSessionLog(HttpContext httpContext)
        {
            try
            {
                var userLanguages = string.Empty;
                if (httpContext.Request.UserLanguages != null)
                    userLanguages = string.Join(",", httpContext.Request.UserLanguages);

                SessionId = httpContext.Session.SessionID;
                Browser = httpContext.Request.Browser.Browser;
                UserHostAddress = httpContext.Request.UserHostAddress;
                UserLanguages = userLanguages;
                MobileDeviceModel = httpContext.Request.Browser.MobileDeviceModel;
                Platform = httpContext.Request.Browser.Platform;
                JavaScript = httpContext.Request.Browser.EcmaScriptVersion.Major >= 1;
                UserAgent = httpContext.Request.UserAgent;

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("Session Log Error {0}", ex.Message), true);
                _isLoaded = false;
            }
        }

        public string SessionId { get; private set; }
        public string Browser { get; private set; }
        public string UserHostAddress { get; private set; }
        public string UserLanguages { get; private set; }
        public string MobileDeviceModel { get; private set; }
        public string Platform { get; private set; }
        public bool JavaScript { get; private set; }
        public string UserAgent { get; private set; }

        public override string ToString()
        {
            if (_isLoaded)
                return
                    string.Format(
                        "Session Id: \"{0}\", Browser: \"{1}\", User Host Address: \"{2}\", User Languages: \"{3}\", Mobile Device Model: \"{4}\", Platform: \"{5}\", User Agent: \"{6}\"",
                        SessionId,
                        Browser,
                        UserHostAddress,
                        UserLanguages,
                        MobileDeviceModel,
                        Platform,
                        UserAgent);

            return base.ToString();
        }
    }
}