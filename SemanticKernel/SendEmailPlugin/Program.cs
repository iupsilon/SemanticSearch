// Create the kernel

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SendEmailPlugin.Configuration;

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>();

IConfiguration config = configurationBuilder.Build();

var openAiConfiguration = config.GetSection("OpenAI").Get<OpenAIConfiguration>();

// Create the kernel
var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
builder.Services.AddOpenAIChatCompletion(openAiConfiguration.ModelId, openAiConfiguration.ApiKey);
builder.Plugins.AddFromType<AuthorEmailPlanner>();
#pragma warning disable SKEXP0001
builder.Plugins.AddFromType<EmailPlugin>();

builder.Services.AddSingleton<IPromptRenderFilter, EmailPlugin>();
builder.Services.AddSingleton<IFunctionInvocationFilter, EmailPlugin>();
builder.Services.AddSingleton<IAutoFunctionInvocationFilter, EmailPlugin>();

#pragma warning restore SKEXP0001


var kernel = builder.Build();

#pragma warning disable SKEXP0001
var functionFilter = kernel.FunctionInvocationFilters;
var promptFilters = kernel.PromptRenderFilters;
var autofunc = kernel.AutoFunctionInvocationFilters;

#pragma warning restore SKEXP0001


// Retrieve the chat completion service from the kernel
IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history
ChatHistory chatMessages = new ChatHistory("""
                                           You are a friendly assistant named Menelao, who likes to follow the rules. You will complete required steps
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
    OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        Temperature = 1
    };
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    await foreach (var content in result)
    {
        if (content.Role.HasValue)
        {
            System.Console.Write("Assistant > ");
        }
        
        System.Console.Write(content.Content);
        fullMessage += content.Content;
    }
    System.Console.WriteLine();

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);
}

