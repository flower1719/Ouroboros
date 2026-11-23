using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    public record AiChatStreamChunk(string DeltaText,
    string? FinishReason = null,
    AiTokenUsage? Usage = null);
}
