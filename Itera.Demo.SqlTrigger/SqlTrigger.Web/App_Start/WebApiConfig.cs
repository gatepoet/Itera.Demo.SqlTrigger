using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SqlTrigger.Data;
using SqlTrigger.Web.Controllers;

namespace SqlTrigger.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
    public static class ListenerConfig
    {
        public static void Start()
        {
            var conn = ConfigurationManager.ConnectionStrings["FishyQueue"].ConnectionString;
            var listener = new FishListener("TargetQueue", conn);
            Notifyer = new SignalRNotifyer(listener);
        }

        private static SignalRNotifyer Notifyer;
    }

    public class SignalRNotifyer
    {
        private readonly IListener<FishUpdatedMessage> _listener;

        public SignalRNotifyer(IListener<FishUpdatedMessage> listener)
        {
            _listener = listener;
            _listener.OnMessageRecieved += MessageRecieved;
            _listener.Start();
        }

        void MessageRecieved(object sender, FishUpdatedMessage e)
        {
            GlobalHost.ConnectionManager.GetHubContext("FishHub").Clients.All.fishUpdated(e);
        }
    }

    public class FishListener : IListener<FishUpdatedMessage>
    {
        private Listener _listener;
        private const string MessageType = "http://blog.maskalik.com/RequestMessage";
        private readonly string _queue;
        private readonly string _connectionString;

        public FishListener(string queue, string connectionString)
        {
            _queue = queue;
            _connectionString = connectionString;
        }

        public void Start()
        {
            _listener = new Listener(new Broker(_connectionString), _queue);
            _listener.OnMessageRecieved += MessageRecieved;
            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
            _listener = null;
        }

        public event EventHandler<FishUpdatedMessage> OnMessageRecieved;

        private void MessageRecieved(object sender, SqlEventArgs e)
        {
            if (OnMessageRecieved == null) return;

            if (e.MessageType == MessageType)
            {
                e.Message.Descendants("INSERTED")
                 .Select(el => new FishUpdatedMessage(el))
                 .ToList()
                 .ForEach(m => OnMessageRecieved(this, m));
            }
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }

    public static class XElementExtensions
    {
        public static int GetInt(this XElement element, string name)
        {
            var xElement = element.Element(name);
            if (xElement == null)
            {
                throw new XmlException("Could not find element '" + name + "' in '"+ element.Name +"'.");
            }
            return Convert.ToInt32(xElement.Value);
        }
    }

    public class FishUpdatedMessage
    {
        public FishUpdatedMessage(XElement element)
        {
            this.TypeId = element.GetInt("TypeId");
            this.Count = element.GetInt("Count");
        }

        public int Count { get; private set; }

        public int TypeId { get; private set; }
    }



}
