using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public class User
    {
        public string id;
        public string smartAccountAddress;
        public string email;
    }

    public partial class API
    {
        public async Task<User> GetUser()
        {
            var response = await Get("/users/me");
            return JsonConvert.DeserializeObject<User>(response);
        }
    }
}
