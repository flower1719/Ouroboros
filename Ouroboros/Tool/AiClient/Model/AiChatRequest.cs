using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    //AI聊天请求体模型
    public class AiChatRequest
    {
        public required IReadOnlyList<AiMessage> Messages { get; init; }
        public string? Model { get; init; }
        public double? Temperature { get; init; }
        public int? MaxTokens { get; init; }
        public bool JsonMode { get; init; } = false;
        // 预留扩展字典
        public Dictionary<string, object>? ExtraParameters { get; init; }
    }
}
