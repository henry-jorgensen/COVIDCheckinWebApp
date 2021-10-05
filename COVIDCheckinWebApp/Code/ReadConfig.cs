using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace COVIDCheckinWebApp.Code
{
    public static class ReadConfig
    {
        public static string ReadConnection(string key)
        {
            var all_text = File.ReadAllText("appsettings.json");

            JObject jobj = JsonConvert.DeserializeObject<JObject>(all_text);

            var connection = jobj["ConnectionStrings"][key].Value<string>();

            return connection;
        }
    }
}
