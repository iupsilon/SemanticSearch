using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;

[Experimental("SKEXP0001")]
public class EmailPlugin : IPromptRenderFilter, IFunctionInvocationFilter, IAutoFunctionInvocationFilter
{
    [KernelFunction]
    [Description("Sends an email to a recipient.")]
    public async Task SendEmailAsync(
        Kernel kernel,
        [Description("Semicolon delimitated list of emails of the recipients")]
        string recipientEmails,
        string subject,
        string body
    )
    {
        if (!recipientEmails.Contains("@"))
        {
            throw new InvalidOperationException($"Email non spedita perché il destinatario non è corretto: {recipientEmails}");
        }

        // Add logic to send an email using the recipientEmails, subject, and body
        // For now, we'll just print out a success message to the console
        Console.WriteLine($"[-- Email sent! Recipient: {recipientEmails}{Environment.NewLine}{subject}{Environment.NewLine}{body} --]");
    }

    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // Example: get function information
        var functionName = context.Function.Name;

        await next(context);

        // Example: override rendered prompt before sending it to AI
        //context.RenderedPrompt = "Safe prompt";
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Something went wrong during function invocation: {exception.Message}");
            context.Result = new FunctionResult(context.Result, exception.Message);

            // Example: Rethrow another type of exception if needed
            // throw new InvalidOperationException("New exception");
        }
    }

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        // Example: get function information
        var functionName = context.Function.Name;

        // Example: get chat history
        var chatHistory = context.ChatHistory;

        // Example: get information about all functions which will be invoked
        var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last());

        // Example: get request sequence index
        Console.WriteLine($"Request sequence index: {context.RequestSequenceIndex}");

        // Example: get function sequence index
        Console.WriteLine($"Function sequence index: {context.FunctionSequenceIndex}");

        // Example: get total number of functions which will be called
        Console.WriteLine($"Total number of functions: {context.FunctionCount}");

        // Calling next filter in pipeline or function itself.
        // By skipping this call, next filters and function won't be invoked, and function call loop will proceed to the next function.
        await next(context);

        // Example: get function result
        var result = context.Result;

        // Example: override function result value
        //context.Result = new FunctionResult(context.Result, "+++++ Result from auto function invocation filter");
        //context.Result = new FunctionResult(context.Result, result);

        // Example: Terminate function invocation
        context.Terminate = true;
    }
}