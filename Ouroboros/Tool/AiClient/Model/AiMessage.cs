using Ouroboros.Tool.AiClient.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    //单条msg
    public record AiMessage(
        AiRole Role,
        string Content,
        string? ToolCallId = null);
}
