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
    public void tt()
    {
        var connectionString = ConfigurationManager.ConnectionStrings["FishyQueue"].ConnectionString;
        var queue = "NewFishQueue";
        CanRequestNotifications();

         SqlDependency.Start(connectionString, queue);
        var c = new SqlCommand("SELECT ID from dbo.Fish", new SqlConnection(connectionString));
        var dep = new SqlDependency(c);
        dep.OnChange += (sender, args) => System.Console.WriteLine(args.Info + ":" + args.Source + ":" + args.Type + "::" + sender.ToString());
        c.Connection.Open();
        c.ExecuteReader();
        c.Connection.Close();
       var repo = new FishRepository();
        repo.AddFish(0, 3);

        Thread.Sleep(10000);
        SqlDependency.Stop(connectionString, queue);
    }
}