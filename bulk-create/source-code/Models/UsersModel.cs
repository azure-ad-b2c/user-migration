using Newtonsoft.Json;

namespace AADB2C.BulkCreate
{
    public class UsersModel
    {
        public UserModel[] Users { get; set; }

        public static UsersModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON, typeof(UsersModel)) as UsersModel;
        }
    }
}