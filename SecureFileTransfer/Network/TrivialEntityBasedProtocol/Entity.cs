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

        public bool Responded { get; protected set; }


        [JsonIgnore]
        public TEBPProvider Provider { get; set; }

        public Entity(int identifier, bool requiresAnswer)
        {
            Identifier = identifier;
            RequiresAnswer = requiresAnswer;
            Responded = false;
        }
    }
}
