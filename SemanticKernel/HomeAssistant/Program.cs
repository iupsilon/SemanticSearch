// Create the kernel

using HomeAssistant.Configuration;
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


var azureOpenAiConfiguration = config.GetSection("AzureOpenAI").Get<AzureOpenAIConfiguration>();

// Create the kernel
var builder = Kernel.CreateBuilder();
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Information).AddDebug().AddConsole());
builder.Services.AddAzureOpenAIChatCompletion(deploymentName: azureOpenAiConfiguration.DeploymentName, endpoint: azureOpenAiConfiguration.Endpoint, apiKey: azureOpenAiConfiguration.ApiKey);

builder.Plugins.AddFromType<LightsPlugin>("Lights");
builder.Plugins.AddFromType<TemperaturePlugin>("Temperature");
builder.Plugins.AddFromType<SmokeDetectorPlugin>("SmokeDetector");
builder.Plugins.AddFromType<FirePreventionPlugin>("FirePrevention");
var kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// 2. Enable automatic function calling
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var history = new ChatHistory();

history.AddMessage(AuthorRole.System, @"Sei un assistente gentile e preciso. Hai il compito di gestire l'automazione della casa. 
Assicurati che quando il padrone è in casa ci sia la luce più indicata in base all'attività in corso. 
I residenti della casa preferiscono avere una temperatura media intorno ai 21 gradi centigradi; assicurati che la temperatura nelle varie stanze sia sempre gradevole ma mai superiore ai 23 gradi");

string userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine() ?? string.Empty;

    // Add user input
    history.AddUserMessage(userInput);

    // 3. Get the response from the AI with automatic function calling
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (!string.IsNullOrWhiteSpace(userInput));