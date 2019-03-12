using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AADB2C.JITUserMigration.Models
{
    public class OutputClaimsModel
    {
        // Demo: List of security groups the user is member of
        public string objectId { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
