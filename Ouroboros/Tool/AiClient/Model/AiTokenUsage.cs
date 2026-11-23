using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    public record AiTokenUsage(int InputTokens, int OutputTokens)
    {
        public int TotalTokens => InputTokens + OutputTokens;
    }
}
