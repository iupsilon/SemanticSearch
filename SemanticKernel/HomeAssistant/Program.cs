// Create the kernel

using HomeAssistant.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>();

IConfiguration config = configurationBuilder.Build();


// Create the kernel
var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Debug).AddDebug().AddConsole());
#pragma warning disable SKEXP0010
builder.Services.AddOpenAIChatCompletion(modelId: "Model", endpoint: new Uri("http://localhost:1234"), apiKey: "XXXXXXXXXXXXXX");
#pragma warning restore SKEXP0010

builder.Plugins.AddFromType<LightsPlugin>("Lights");
builder.Plugins.AddFromType<TemperaturePlugin>("Temperature");
// builder.Plugins.AddFromType<SmokeDetectorPlugin>("SmokeDetector");
// builder.Plugins.AddFromType<FirePreventionPlugin>("FirePrevention");
var kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// 2. Enable automatic function calling
OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    Temperature = 0.8,
};
var history = new ChatHistory();

history.AddMessage(AuthorRole.System,
    @"Sei un assistente per la domotica della casa. Hai il compito di gestire l'automazione della casa per garantire il comfort del residente. 
Assicurati che quando il padrone è in casa ci sia la luce più indicata in base all'attività in corso. 
La temperatura media gradita è di 21 gradi. Quando mi allontano per molto tempo da una stanza, provvedi ad abbassare la temperatura ed assicurarti che le luci siano spente. 
Esegui le operazioni che ritieni necessarie in autonomia e rispondi in modo conciso ed essenziale");

string userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine() ?? string.Empty;

    // Add user input to history
    history.AddUserMessage(userInput);

    // 3. Get the response from the AI using streaming
    // Use the streaming method, which returns an IAsyncEnumerable
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel,
        new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token);

    // The result is an IAsyncEnumerable, so we can iterate through the tokens
    // as they arrive.
    string assistantMessage = "";
    await foreach (var content in result)
    {
        Console.Write(content);
        assistantMessage += content.Content ?? string.Empty;
    }

    Console.WriteLine(); // Add a new line after the streamed response

    // Add the complete message from the agent to the chat history
    history.AddMessage(AuthorRole.Assistant, assistantMessage);
} while (!string.IsNullOrWhiteSpace(userInput));