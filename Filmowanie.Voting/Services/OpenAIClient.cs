using Filmowanie.Abstractions.Configuration;
using Filmowanie.Voting.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Responses;

namespace Filmowanie.Voting.Services;
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
internal sealed class OpenAIClient : IOpenAIClient
{
    private readonly ILogger<OpenAIClient> log;
    private readonly OpenAIOptions options;

    public OpenAIClient(IOptions<OpenAIOptions> options, ILogger<OpenAIClient> log)
    {
        this.log = log;
        this.options = options.Value;
    }

    public async Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        if (!this.options.Enabled)
        {
            this.log.LogWarning($"{nameof(OpenAIClient)} is disabled!");
            return string.Empty;
        }

        try
        {
            var client = new OpenAIResponseClient(model: "gpt-5", apiKey: this.options.ApiKey);
            var options = new ResponseCreationOptions { TruncationMode = ResponseTruncationMode.Auto, ReasoningOptions = new() { ReasoningEffortLevel = ResponseReasoningEffortLevel.High } };
            var response = await client.CreateResponseAsync(prompt, options, cancellationToken);

            if (response == null)
            {
                this.log.LogError("Error occurred when trying to fetch response from openai. Falling back to hardcoded value. Got content: ");
                return "Something went wrong with fetching data :(";
            }
            var result = response.Value.GetOutputText();
            return result;
        }
        catch (Exception ex)
        {
            this.log.LogError(ex, "Error occurred when trying to fetch response from openai. Falling back to hardcoded value.");
            return "Something went wrong with fetching data :(";
        }

    }
}
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
