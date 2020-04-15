using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace AADB2C.BulkCreate
{
    public class ObjectIdentity
    {

        [JsonProperty(PropertyName = "signInType")]
        public string SignInType { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [JsonProperty(PropertyName = "issuerAssignedId")]
        public string IssuerAssignedId { get; set; }
    }
    public class UserModel : User
    {
        [JsonProperty(PropertyName = "password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "identities", NullValueHandling = NullValueHandling.Ignore)]
        public List<ObjectIdentity> Identities { set; get; }


        public void SetB2CProfile(string TenantName)
        {
            this.PasswordProfile = new PasswordProfile
            {
                ForceChangePasswordNextSignIn = false,
                Password = this.Password,
                ODataType = null
            };

            this.Password = null;
            this.ODataType = null;

            foreach (var item in this.Identities)
            {
                if (item.SignInType == "emailAddress" || item.SignInType == "userName")
                {
                    item.Issuer = TenantName;
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}