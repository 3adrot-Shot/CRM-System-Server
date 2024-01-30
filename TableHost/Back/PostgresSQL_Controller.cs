using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableHost.Back
{
    internal class PostgreSQL_Controller
    {
        public static NpgsqlConnection PGSQL = null;
        static string ConPSQL = $"Server={Properties.Settings.Default.ParamServer};Port={Properties.Settings.Default.ParamPort};User Id={Properties.Settings.Default.ParamUserId};Password={Properties.Settings.Default.Password};Database={Properties.Settings.Default.Database};";
        public static bool Connect()
        {
            try
            {
                PGSQL = new NpgsqlConnection(connectionString: ConPSQL);
                PGSQL.Open();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static void Close()
        {
            PGSQL.Close();
        }
        public static string Auth(string Login, string Password, string TableName = "user")
        {
            Close();
            var cmd = new NpgsqlCommand();
            if (PGSQL.State != System.Data.ConnectionState.Open)
            {
                Connect();
            }
            cmd.Connection = PGSQL;
            cmd.CommandText = $"SELECT id, last_name, first_name, patronymic, email, avatar_image_id, permissions, active FROM \"public\".\"user\" WHERE \"login\" = '" + @Login + "' AND \"password\" = '" + @Password + "' LIMIT 2;";
            int sums = 0;
            try
            {
                NpgsqlDataReader reader = cmd.ExecuteReader();
                string[] saveParams = new string[8];
                while (reader.Read())
                {
                    sums++;
                    for (int i = 0; i < saveParams.Length; i++)
                    {
                        saveParams[i] = $"{reader[i]}";
                    }
                }
                if (sums == 1)
                {
                    var jsonStructure = new
                    {
                        status = "Success",
                        user_AuthId = "" + saveParams[0],
                        user_last_name = "" + saveParams[1],
                        user_first_name = "" + saveParams[2],
                        user_patronymic = "" + saveParams[3],
                        user_email = "" + saveParams[4],
                        user_avatar_image_id = "" + saveParams[5],
                        user_permissions = "" + saveParams[6],
                        user_active = "" + saveParams[7]
                    };
                    return JsonConvert.SerializeObject(jsonStructure, Formatting.Indented);
                }
                else
                {
                    return "{False}";
                }
            }
            catch (Exception ex)
            {
                return "{Error}";
            }
        }
        public static string GetProjectList(string id)
        {
            return null;
        }
    }
}