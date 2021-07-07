using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ConsoleDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            NotificationExample ne = new NotificationExample();
            ne.StartNotification();
            ne.StopNotification();
        }
    }
    public class NotificationExample
    {
        private delegate void RateChangeNotification(DataTable table);
        private SqlDependency dependency;
        string ConnectionString = "database sonnection string";
       
        public void StartNotification()
        {
            SqlDependency.Start(this.ConnectionString,"QueueName");
            SqlConnection connection = new SqlConnection(this.ConnectionString);
            connection.Open();

            SqlCommand command=new SqlCommand();
            command.CommandText="SQL Statement";
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            this.dependency = new SqlDependency(command);
            dependency.OnChange += new OnChangeEventHandler(OnRateChange);

        }
        private void OnRateChange(object s,SqlNotificationEventArgs e)
        {
          //Write code for you task
        }
        public void StopNotification()
        {
            SqlDependency.Stop(this.ConnectionString,"QueueName");
        }
    }
}
