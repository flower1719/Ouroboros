namespace Ouroboros
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var aiClientOptions = new Tool.AiClient.Model.AiClientOptions
            {
                Endpoint = "https://api.deepseek.com",
                ApiKey = "",
                DefaultModel = "deepseek-flash"
            };

            var aiClient = new Tool.AiClient.AiChatClient(aiClientOptions);
            //程序循环
            while (true)
            {
                var input = Console.ReadLine();
                var message = new Tool.AiClient.Model.AiMessage(
                    Tool.AiClient.Enum.AiRole.User,
                    input ?? string.Empty
                );

                var request = new Tool.AiClient.Model.AiChatRequest
                {
                    Messages = new List<Tool.AiClient.Model.AiMessage> { message },
                    Model = aiClientOptions.DefaultModel,
                    Temperature = 0.7,
                    MaxTokens = 150
                };
                var response = await aiClient.CompleteAsync(request);
                Console.WriteLine($"AI: {response.Content}");
            }
        }
    }
}
