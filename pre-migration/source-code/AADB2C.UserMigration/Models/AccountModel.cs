using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.UserMigration.Models
{
    public class LocalAccountsModel
    {
        public string userType;
        public List<AccountModel> Users;

        public LocalAccountsModel()
        {
            Users = new List<AccountModel>();
        }

        /// <summary>
        /// Parse JSON string into UsersModel
        /// </summary>
        public static LocalAccountsModel Parse(string JSON)
        {
            return  JsonConvert.DeserializeObject(JSON, typeof(LocalAccountsModel)) as LocalAccountsModel;
        }
        /// <summary>
        /// Serialize the object into Json string
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class AccountModel
    {
        // Local account attributes
        public string signInName { set; get; }
        public string password { set; get; }

        // Social account attributes
        public string issuer { set; get; }
        public string issuerUserId { set; get; }

        // Local as social accont attributes
        public string email { set; get; }
        public string displayName { set; get; }
        public string firstName { set; get; }
        public string lastName { set; get; }
    }
}
