using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    public class AiClientOptions
    {
        public required string Endpoint { get; set; } 
        public required string ApiKey { get; set; }
        public required string DefaultModel { get; set; }
    }
}
