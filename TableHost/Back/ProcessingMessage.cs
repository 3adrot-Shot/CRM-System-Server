using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TableHost.Back
{
    internal class ProcessingMessage
    {
        // Определение запроса в нужное русло
        public static string EnterRespons(string jsonString)
        {
            PostgreSQL_Controller.Connect();
            JObject jsonObject = JObject.Parse(jsonString);
            if (jsonObject["Login"] != null)
            {
                return Login(jsonString);
            }
            return "Null";
        }

        // Обработка запроса на логин
        public static string Login(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);
            string propertyJsonObjects = "";
            foreach (var property in jsonObject.Properties())
            {
                propertyJsonObjects += $"{property.Name}: {property.Value}";
            }
            string login = jsonObject["Login"][0]["Login"].ToString();
            string password = jsonObject["Login"][0]["Password"].ToString();
            string platform = jsonObject["ClientInfo"][0]["Platform"].ToString();
            string version = jsonObject["ClientInfo"][0]["Version"].ToString();
            string Response = PostgreSQL_Controller.Auth(login, password);
            return Response;
        }

        //var jsonStructure = new
        //{
        //    Login = new[]
        //    {
        //            new
        //            {
        //                Login = "LoginUser",
        //                Password = "PasswordUser"
        //            }
        //        },
        //    ClientInfo = new[]
        //    {
        //            new
        //            {
        //                Platform = "Windows",
        //                Version = "v0.0.1b"
        //            }
        //        }
        //};

    }

    // Проверка актуальной версии программы 
    internal class CheckerPlatform
    {
        public bool PlatformCheck(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);
            if (jsonObject["Platform"] != null)
            {
                return CheckVersion(jsonObject["ClientInfo"][0]["Version"].ToString(), jsonObject["ClientInfo"][0]["Platform"].ToString());
            }
            else
            {
                return false;
            }
        }
        public bool CheckVersion(string version, string platform)
        {
            if (platform == "Windows") 
            {
                if (version == "v0.0.1b")
                {
                    return true; 
                }
                else
                {
                    return false;
                }
            }
            if (platform == "Linux")
            {
                if (version == "Not created")
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            if (platform == "Android")
            {
                if (version == "Not created")
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            if (platform == "IOS")
            {
                if (version == "Not created")
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
