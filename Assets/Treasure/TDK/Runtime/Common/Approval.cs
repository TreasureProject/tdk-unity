using System;
using Newtonsoft.Json;

namespace Treasure
{
    [Serializable]
    public class Approval
    {
        [JsonProperty("operator")]
        public string operatorAddress;
        public bool approved;
    }
}
