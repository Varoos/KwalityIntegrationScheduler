using Newtonsoft.Json;
using KwalityIntegrationScheduler.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static KwalityIntegrationScheduler.Form1;

namespace KwalityIntegrationScheduler
{
    public class GetExternalData
    {
        static string ESerName = ConfigurationManager.AppSettings["ExternalServerName"];
        static string EDBName = ConfigurationManager.AppSettings["ExternalDBName"];
        static string EUID = ConfigurationManager.AppSettings["ExternalUserName"];
        static string EPWD = ConfigurationManager.AppSettings["ExternalPassword"];
        static string connection = $"data source={ESerName};initial catalog={EDBName};User ID={EUID};Password={EPWD};integrated security=True;MultipleActiveResultSets=True";
        static SqlConnection con = new SqlConnection(connection);

        

        public static DataSet getFn(string Operation)
        {
            string sql = "";
            sql = $@"
                exec pCore_CommonSp @Operation={Operation}";

            DataSet dst = GetData(sql);
            return dst;

        }
        public static int setFn(string Operation, string DocNo)
        {
            string sql = "";
            sql = $@"
                exec pCore_CommonSp @Operation={Operation},@p1='{DocNo}'";

            int f = Update(sql);
            return f;

        }

        public static DataSet GetData(string Query)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand(Query, con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            DataSet dst = ds;
            con.Close();
            return dst;
        }


        public static int Update(string Vouc)
        {
            int result = 0;
            using (SqlConnection connect = new SqlConnection(connection))
            {
                string sql = $"{Vouc}";
                using (SqlCommand command = new SqlCommand(sql, connect))
                {
                    connect.Open();
                    result = command.ExecuteNonQuery();
                    connect.Close();
                }
            }
            return result;
        }
    }
}
