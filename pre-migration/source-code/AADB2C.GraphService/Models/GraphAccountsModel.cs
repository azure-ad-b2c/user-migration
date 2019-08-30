using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.GraphService
{
    public class GraphAccounts
    {
        public string odatametadata { get; set; }
        public List<GraphAccountModel> value { get; set; }

        public static GraphAccounts Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON.Replace("odata.metadata", "odatametadata"), typeof(GraphAccounts)) as GraphAccounts;
        }
    }
}
