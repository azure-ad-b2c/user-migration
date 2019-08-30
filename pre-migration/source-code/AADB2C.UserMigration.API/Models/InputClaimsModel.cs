using Newtonsoft.Json;


namespace AADB2C.UserMigration.API.Models
{
    public class InputClaimsModel
    {
        public string email { get; set; }
        public string password { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}