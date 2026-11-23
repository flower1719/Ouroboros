using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Tool.AiClient.Model
{
    //AI聊天请求体模型
    public class AiChatRequest
    {
        //消息列表
        public required IReadOnlyList<AiMessage> Messages { get; init; }
        //模型
        public string? Model { get; init; }
        //温度
        public double? Temperature { get; init; }
        //最大token
        public int? MaxTokens { get; init; }
        public bool JsonMode { get; init; } = false;
        // 预留扩展字典
        public Dictionary<string, object>? ExtraParameters { get; init; }
    }
}
