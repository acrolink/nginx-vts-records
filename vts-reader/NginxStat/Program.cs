using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NginxStat
{
    class Program
    {
		
		// Serve Nginx Statistics over a local ip address, e.g. 127.0.0.25
		private static readonly string BASE_URL = "http://127.0.0.25";
		
        private static readonly string json_data_URL = BASE_URL + "/status/format/json";
		
        static void Main(string[] args)
        {

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client.OpenRead(json_data_URL);
            StreamReader reader = new StreamReader(data);
            string json_string = reader.ReadToEnd();
			// Months are stored in the database in the format "2016-08-01 00:00:00" i.e. August 2016
            string month = DateTime.Now.ToString("yyyy-MM-01 00:00:00");
            data.Close();
            reader.Close();


			// to resest stats at month start
			// curl --request GET 'http://127.0.0.25/status/control?cmd=reset&group=*'
			
			// Get Nginx start time as a Unix timestamp, we need this to collect reliable statistics across possible Nginx service restarts.
			string nginx_start_time = null;
			
			// In order to do this, we use a .sh script
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/local/scripts/nginx_start_time.sh",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

			// Run the script
            proc.Start();
			
			// Store the Nginx service start tuime
            while (!proc.StandardOutput.EndOfStream)
            {
                nginx_start_time = proc.StandardOutput.ReadLine();
                Console.WriteLine("Nginx Started at: " + nginx_start_time);
            }

			// Convert Json string to c# object
            dynamic statistical_data = JsonConvert.DeserializeObject(json_string);

			// Connection string to MySQL database
            string ConnectionString = "server=127.0.0.1;uid=nginxstat;pwd=nginxstat;database=nginxstat;";
			
			// Insert the statistical data into the databse
			StringBuilder sCommand_new = new StringBuilder("INSERT INTO logs (month, zone, outBytes, inBytes, requests, nginx_start) VALUES ");
			using (MySql.Data.MySqlClient.MySqlConnection mConnection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
            {
                List<string> Rows = new List<string>();
               foreach (var zone in statistical_data.serverZones)
                {
                    Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}', '{5}')", MySqlHelper.EscapeString(month), MySqlHelper.EscapeString(zone.Name), zone.First.outBytes, zone.First.inBytes, zone.First.requestCounter, nginx_start_time));
                }		
				
                sCommand_new.Append(string.Join(",", Rows));
				sCommand_new.Append("ON DUPLICATE KEY UPDATE `outBytes`=VALUES(`outBytes`), `inBytes`=VALUES(`inBytes`), `requests`=VALUES(`requests`)");
                sCommand_new.Append(";");
                mConnection.Open();
				
                using (MySqlCommand myCmd = new MySqlCommand(sCommand_new.ToString(), mConnection))
                {
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
					Console.WriteLine("Database updated successfully.");
                }
            }
			
        }
    }
}
