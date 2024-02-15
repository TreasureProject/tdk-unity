using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public class Project
    {   
        public string slug;
        public string name;
        public List<string> backendWallets;
        public List<string> callTargets;
        public string icon;
        public string cover;
        public string color;
    }

    public partial class API
    {
        public async Task<Project> GetProjectBySlug(string slug)
        {
            var response = await Get($"/projects/{slug}");
            return JsonConvert.DeserializeObject<Project>(response);
        }
    }
}
