using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Permissions;
using System.Threading;
using NUnit.Framework;
using SqlTrigger.Data;

[TestFixture]
public class test
{

    private bool CanRequestNotifications()
    {
        SqlClientPermission permission =
            new SqlClientPermission(
            PermissionState.Unrestricted);
        try
        {
            permission.Demand();
            return true;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    [Test]
    public void tt3()
    {
        var listener = new Listener(new Broker(ConfigurationManager.ConnectionStrings["FishyQueue"].ConnectionString), "TargetQueue");
        listener.OnMessageRecieved += (sender, args) => Console.WriteLine(args);
        listener.Start();
        var repo = new FishRepository();
        repo.AddFish(1, 7);
        Thread.Sleep(500);
        repo.AddFish(1, 8);
        Thread.Sleep(500);
        listener.Stop();
    }

    [Test]
    public void tt2()
    {
        var broker = new Broker(ConfigurationManager.ConnectionStrings["FishyQueue"].ConnectionString);
        string msgType;
        string msg;
        Guid group;
        Guid handle;
        broker.Receive("ExternalActivatorQueue", out msgType, out msg, out group, out handle);

        Console.WriteLine("{0}:{1}:{2}:{3}", msgType, msg, group, handle);
        //var repo = new FishRepository();
        //repo.AddFish(1, 7);
    }

    [Test]
    public void tt()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["FishyQueue"].ConnectionString;
        var queue = "ExternalActivatorQueue";
        CanRequestNotifications();

         SqlDependency.Start(connectionString);
        var c = new SqlCommand("SELECT ID from dbo.Fish", new SqlConnection(connectionString));
        var dep = new SqlDependency(c);
        dep.OnChange += (sender, args) => System.Console.WriteLine(args.Info + ":" + args.Source + ":" + args.Type + "::" + sender.ToString());
        c.Connection.Open();




       var repo = new FishRepository();
        repo.AddFish(1, 3);

        Thread.Sleep(10000);
        c.Connection.Close(); 
        SqlDependency.Stop(connectionString, queue);
    }
}