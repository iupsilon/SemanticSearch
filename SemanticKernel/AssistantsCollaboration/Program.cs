// Copyright (c) Kevin BEAUGRAND. All rights reserved.

using AssistantsCollaboration.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernel.Assistants;
using SemanticKernel.Assistants.Extensions;
using Spectre.Console;

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>();

IConfiguration config = configurationBuilder.Build();

var openAiConfiguration = config.GetSection("OpenAI").Get<OpenAIConfiguration>();

using var loggerFactory = LoggerFactory.Create(logging =>
{
    logging
        .AddDebug()
        .AddConsole()
        .AddConfiguration(config.GetSection("Logging"));
});

AnsiConsole.Write(new FigletText($"AutoGen").Color(Color.Green));
AnsiConsole.WriteLine("");

IAssistant assistant = null!;

AnsiConsole.Status().Start("Initializing...", ctx =>
{
    string modelId = openAiConfiguration.ModelId;
    string key = openAiConfiguration.ApiKey;


    var butlerKernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(modelId, key);

    butlerKernelBuilder.Services.AddSingleton(loggerFactory);

    var butlerKernel = butlerKernelBuilder.Build();


    butlerKernel.ImportPluginFromAssistant(CreateCodeInterpreter(modelId, key));

    assistant = AssistantBuilder.FromTemplate("-----")
        .WithKernel(butlerKernel)
        .Build();
});

IAssistant CreateCodeInterpreter(string modelId, string key)
{
    var kernel = Kernel.CreateBuilder()
        //.AddAzureOpenAIChatCompletion(azureOpenAIDeploymentName, azureOpenAIEndpoint, azureOpenAIKey)
        .AddOpenAIChatCompletion(modelId, key)
        .Build();

    return AssistantBuilder.FromTemplate("dasdsada")
        .WithKernel(kernel)
        .Build();
}


var options = config.GetRequiredSection("CodeInterpreter");

var thread = assistant.CreateThread();

while (true)
{
    var prompt = AnsiConsole.Prompt(new TextPrompt<string>("User > ").PromptStyle("teal"));

    await AnsiConsole.Status().StartAsync("Creating...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));
        ctx.Status($"Processing ...");

        var answer = await thread.InvokeAsync(prompt).ConfigureAwait(true);

        AnsiConsole.MarkupInterpolated($"AutoGen > [cyan]{answer.Content!}[/]");
    });
}