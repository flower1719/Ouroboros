using Ouroboros.Tool.AiClient.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ouroboros.Memory
{
    // 代表一次对话的上下文帧
    public class ConversationFrame
    {
        // 对话帧的唯一标识符
        public int FrameId { get; set; }
        // 对话帧的摘要信息，用于快速了解对话内容
        public string Summary { get; set; } = string.Empty;
        //对话时间戳，记录对话发生的时间
        public DateTime DateTime { get; set; } = DateTime.Now;
        // 对话的详细内容，可能包含用户输入、AI响应等信息
        public Dictionary<AiRole,string> Massage { get; set; } =  new Dictionary<AiRole,string>();
    }
}
