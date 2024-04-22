// Create the kernel

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;

var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());

builder.Services.AddHuggingFaceChatCompletion("TheBloke/Mistral-7B-Instruct-v0.2-GGUF/mistral-7b-instruct-v0.2.Q6_K.gguf", new Uri("http://localhost:1234"));
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
builder.Plugins.AddFromType<AuthorEmailPlanner>();
builder.Plugins.AddFromType<EmailPlugin>();
Kernel kernel = builder.Build();

// Retrieve the chat completion service from the kernel
IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history
ChatHistory chatMessages = new ChatHistory("""
                                           You are a friendly assistant who likes to follow the rules. You will complete required steps
                                           and request approval before taking any consequential actions. If the user doesn't provide
                                           enough information for you to complete a task, you will keep asking questions until you have
                                           enough information to complete the task.
                                           """);

// Start the conversation
while (true)
{
    // Get user input
    System.Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    var result =  chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: new HuggingFacePromptExecutionSettings
        {
            Temperature = 1,
            Details = true,
            UseCache = true,
        },
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    await foreach (var content in result)
    {
        // if (content.Role.HasValue)
        // {
        //     System.Console.Write("Assistant > ");
        // }

        System.Console.Write(content.Content);
        fullMessage += content.Content;
    }

    System.Console.WriteLine();

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);
}