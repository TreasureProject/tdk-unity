using System;
using System.Collections.Generic;

namespace Treasure
{
    [Serializable]
    public class Token
    {
        public struct Attribute
        {
            public string type;
            public string value;
        }

        public string address;
        public int tokenId;
        public string name;
        public string image;
        public string imageAlt;
        public List<Attribute> attributes;
    }

    [Serializable]
    public class InventoryToken : Token
    {
        public string user;
        public double balance;
    }

}
