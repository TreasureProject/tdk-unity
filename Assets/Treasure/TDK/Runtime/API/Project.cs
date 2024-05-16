using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public string backendWallet
        {
            get
            {
                return backendWallets.Count > 0 ? backendWallets[0].ToLowerInvariant() : null;
            }
        }

        public IEnumerable<string> requestedCallTargets
        {
            get
            {
                return callTargets.Select(callTarget => callTarget.ToLowerInvariant());
            }
        }
    }

    public partial class API
    {
        public async Task<Project> GetProjectBySlug(string slug, RequestOverrides? overrides = null)
        {
            var response = await Get($"/projects/{slug}", overrides);
            return JsonConvert.DeserializeObject<Project>(response);
        }
    }
}
