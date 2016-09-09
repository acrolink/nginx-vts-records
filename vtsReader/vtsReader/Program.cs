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

namespace vtsReader
{
	class Program
	{

		// Nginx Statistics server base url and various endpoints
		private static readonly string BASE_URL = "http://" + System.Configuration.ConfigurationManager.AppSettings["statisticsServer"];
		private static readonly string json_data_URL = BASE_URL + "/status/format/json";
		private static readonly string reset_counters_URL = BASE_URL + "/status/control?cmd=reset&group=*";

		// MySQL information
		private static readonly string MySQL_server = System.Configuration.ConfigurationManager.AppSettings["MySQL_server"];	
		private static readonly string MySQL_DB = System.Configuration.ConfigurationManager.AppSettings["MySQL_DB"];
		private static readonly string MySQL_user = System.Configuration.ConfigurationManager.AppSettings["MySQL_user"];
		private static readonly string MySQL_pass = System.Configuration.ConfigurationManager.AppSettings["MySQL_pass"];

		// Connection string to MySQL database
		private static readonly string ConnectionString = "server=" + MySQL_server + ";uid=" + MySQL_user +";pwd=" + MySQL_pass + ";database=" + MySQL_DB + ";";


		static void Main(string[] args)
		{

			string month_now = DateTime.Now.ToString("yyyy-MM-01 00:00:00");
			string month_now_mini = DateTime.Now.ToString("yyyy-MM");

			string last_readings_year_month = null;

			using (MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
			{
				connection.Open();
				using (MySqlCommand cmd = new MySqlCommand("select `value` from `meta` where `entity` = 'last_readings_year_month' LIMIT 1;", connection))
				{
					MySqlDataReader r = cmd.ExecuteReader();
					if (r.Read()) {
						last_readings_year_month = r["value"].ToString();
					}
					Console.WriteLine("This month: " + month_now_mini);
					Console.WriteLine("last_readings_year_month: " + last_readings_year_month);
				}
			}

			// Reset counters if new month had begun
			if (month_now_mini != last_readings_year_month) {

				Console.WriteLine("New month..");

				WebClient reset_data_client = new WebClient();
				reset_data_client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				reset_data_client.DownloadString(reset_counters_URL);

				using (MySql.Data.MySqlClient.MySqlConnection update_month_connection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
				{
					update_month_connection.Open();
					MySqlCommand update_month_cmd = update_month_connection.CreateCommand();

					update_month_cmd.CommandText = "UPDATE `meta` SET `value` = @this_month WHERE `entity` = 'last_readings_year_month'";
					update_month_cmd.Parameters.AddWithValue("@this_month", month_now_mini);
					update_month_cmd.ExecuteNonQuery();
					update_month_connection.Close();
				}

				// Wait for 2.5 seconds just to make sure the statistical data got nullified
				System.Threading.Thread.Sleep(2500);

			} else {
				// Same month, do noting..
			}

			WebClient get_data_client = new WebClient();
			get_data_client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
			Stream data = get_data_client.OpenRead(json_data_URL);
			StreamReader reader = new StreamReader(data);
			string json_string = reader.ReadToEnd();
			// Months are stored in the database in the format "2016-08-01 00:00:00" i.e. August 2016
			data.Close();
			reader.Close();

			// Get Nginx start time as a Unix timestamp, we need this to collect reliable statistics across possible Nginx service restarts.
			string nginx_start_time = null;

			// In order to do this, we use a .sh script, create somewhere in the system and make it executable

			/*** nginx_start_time.sh => contents:

			#!/bin/sh
			pid=`ps hf -opid -C nginx | awk '{ print $1; exit }'`
			cmd=`ps -o lstart= -p $pid`
			date -d "$cmd" +'%s'

			 ***/

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

			// Insert the statistical data into the databse
			StringBuilder sCommand_new = new StringBuilder("INSERT INTO logs (month, zone, outBytes, inBytes, requests, nginx_start) VALUES ");
			using (MySql.Data.MySqlClient.MySqlConnection mConnection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
			{
				List<string> Rows = new List<string>();
				foreach (var zone in statistical_data.serverZones)
				{
					Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}', '{5}')", MySqlHelper.EscapeString(month_now), MySqlHelper.EscapeString(zone.Name), zone.First.outBytes, zone.First.inBytes, zone.First.requestCounter, nginx_start_time));
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
