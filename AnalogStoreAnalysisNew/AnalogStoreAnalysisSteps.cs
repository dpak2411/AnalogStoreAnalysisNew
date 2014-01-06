using System;
using System.Collections.Generic;
using AlteryxGalleryAPIWrapper;
using AnalogStoreAnalysis;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AnalogStoreAnalysisNew
{
    [Binding]
    public class AnalogStoreAnalysisSteps
    {
        public string alteryxurl;
        public string _sessionid;
        private string _appid; //= "506f68147ae24a0724305f78";
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;

        // public delegate void DisposeObject();
        //private Client Obj = new Client("https://devgallery.alteryx.com/api/");
        Client Obj = new Client("https://gallery.alteryx.com/api/");
        RootObject jsString = new RootObject();


        [Given(@"alteryx running at ""(.*)""")]
        public void GivenAlteryxRunningAt(string url)
        {
            alteryxurl = url;
        }

        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            _sessionid = Obj.Authenticate(user, password).sessionId;

        }

        [When(@"I run analog store analysis with Address (.*) City (.*) State (.*) Zip (.*) MaxSearchDistance (.*)")]
        public void WhenIRunAnalogStoreAnalysisWithAddressCityStateZipMaxSearchDistance(string address, string city, string state, string zip, string maxSearchDistance)
        {
            string response = Obj.SearchApps("Analog Store Analysis");
            var appresponse = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(response);
            int count = appresponse["recordCount"];
            _userid = appresponse["records"][1]["owner"]["id"];
            _appName = appresponse["records"][1]["primaryApplication"]["fileName"];
            _appid = appresponse["records"][1]["id"];

            jsString.appPackage.id = _appid;
            //  jsString.appPackage.id = "52c15e7120aaf90df8ad1db5";
            jsString.userId = _userid;
            jsString.appName = _appName;

            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);
            
            List<Jsonpayload.Question> questionAnsls = new List<Jsonpayload.Question>();
            questionAnsls.Add(new Jsonpayload.Question("Address", "\"" + address + "\""));
            questionAnsls.Add(new Jsonpayload.Question("City", "\"" + city + "\""));
            questionAnsls.Add(new Jsonpayload.Question("State", "\"" + state + "\""));
            questionAnsls.Add(new Jsonpayload.Question("ZIP", "\"" + zip + "\""));
            questionAnsls.Add(new Jsonpayload.Question("Maximum Search Distance", "\"" + maxSearchDistance + "\""));

          
            jsString.questions.AddRange(questionAnsls);
            jsString.jobName = "Job Name";
            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);
            var jobqueue = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(resjobqueue);
            jobid = jobqueue["id"];

            int counts = 0;
            string status = "";

        CheckValidate:
            System.Threading.Thread.Sleep(1000);
            if (status == "Completed" && counts < 15)
            {
                //string disposition = validationStatus.disposition;
            }
            else if (count < 5)
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                var statusResponse = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(jobstatusresp);
                status = statusResponse["status"];
                goto CheckValidate;
            }

            else
            {
                throw new Exception("Complete Status Not found");

            }
        }

        [Then(@"I see the average sales result (.*)")]
        public void ThenISeeTheAverageSalesResult(string averageSales)
        {
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }
            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            string output = doc.DocumentNode.SelectSingleNode("//span[@class='DefaultNumericText']").InnerHtml;
            
            Assert.AreEqual(averageSales, output);


            
        }
    }
}
