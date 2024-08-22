// Import packages
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace LearnSemanticKernel.AIReceptionist
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Create a kernel with Azure OpenAI chat completion
            var builder = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion("modelId", "endpoint", "apiKey");

            // Add enterprise components
            //builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

            // Build the kernel
            Kernel kernel = builder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Enable planning
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var businessInfoText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),"Data/BusinessInfo.txt"));

            // Create a history store the conversation
            var history = new ChatHistory(businessInfoText);

            // Initiate a back-and-forth chat
            string? userInput;
            do
            {
                // Collect user input
                Console.Write("User > ");
                userInput = Console.ReadLine();

                // Add user input
                history.AddUserMessage(userInput);

                // Get the response from the AI
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

                // Print the results
                Console.WriteLine("Assistant > " + result);

                // Add the message from the agent to the chat history
                history.AddMessage(result.Role, result.Content ?? string.Empty);
            } while (userInput is not null);
        }
    }
}
