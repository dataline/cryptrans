using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public abstract class Entity
    {
        public int Identifier { get; set; }
        public bool RequiresAnswer { get; set; }

        //public int Type { get; set; }
        //
        //public const int TYPE_REQUEST = 1;
        //public const int TYPE_RESPONSE = 2;
        //public const int TYPE_NOTICE = 3;


        [JsonIgnore]
        public TEBPProvider Provider { get; set; }

        public Entity(int identifier, bool requiresAnswer/*, int type*/)
        {
            Identifier = identifier;
            RequiresAnswer = requiresAnswer;
            //Type = type;
        }
    }
}
