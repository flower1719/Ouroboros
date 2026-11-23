using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    public record AiChatResponse(
        string Content,
        string? FinishReason,
        AiTokenUsage? Usage,
        string? ModelUsed = null);
}
