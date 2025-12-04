using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Realtime;
using SantaClausRealtimeChat.Helpers;
using SantaClausRealtimeChat.Utils;

ConsoleHelper.ShowHeader();

RealtimeClient? realtimeClient = null;
ChatClient? chatClient = null;
string realtimeModelName = string.Empty;

// Get the current host
string host =
    ConsoleHelper.SelectFromOptions(
        [Statics.AzureOpenAI, Statics.OpenAI],
        "Select the [yellow]host[/] for the conversation.");

switch (host)
{
    case Statics.AzureOpenAI:
        {
            string endpoint =
                ConsoleHelper.GetUrlFromConsole(
                    "Enter the [yellow]Azure OpenAI[/] endpoint.");

            string apiKey =
               ConsoleHelper.GetStringFromConsole(
                   "Enter your [yellow]Azure OpenAI[/] API key.");

            string chatModelName =
                ConsoleHelper.GetStringFromConsole(
                   "Enter your [yellow]Azure OpenAI[/] chat model name.");

            realtimeModelName =
                ConsoleHelper.GetStringFromConsole(
                   "Enter your [yellow]Azure OpenAI[/] realtime model name.");


            AzureOpenAIClient aoaiClient =
                new(
                    new Uri(endpoint),
                    new ApiKeyCredential(apiKey));

            realtimeClient = aoaiClient
                    .GetRealtimeClient();

            chatClient = aoaiClient
                    .GetChatClient(chatModelName);
        }
        break;

    case Statics.OpenAI:
        {
            string apiKey =
                ConsoleHelper.GetStringFromConsole(
                    "Enter your [yellow]OpenAI[/] API key.");

            string chatModelName =
                ConsoleHelper.SelectFromOptions(
                    [Statics.GPT51Key, Statics.GPT5Key,
                    Statics.GPT5MiniKey, Statics.GPT5NanoKey,
                    Statics.GPT41Key, Statics.GPT41MiniKey,
                    Statics.GPT41NanoKey, Statics.GPT4oKey,
                    Statics.GPT4oMiniKey, Statics.GPT4Key,
                    Statics.GPT4TurboKey, Statics.GPT35TurboKey],
                    "Enter the [yellow]model name[/] for the chat.");

            realtimeModelName =
                ConsoleHelper.SelectFromOptions(
                    [Statics.GptRealtimeMiniModelName,
                    Statics.GptRealtimeModelName],
                    "Enter the [yellow]model name[/] for the real-time conversation.");

            OpenAIClient openAIClient = new(apiKey);

            realtimeClient =
                openAIClient.GetRealtimeClient();

            chatClient =
                openAIClient.GetChatClient(chatModelName);
        }
        break;
}

if (realtimeClient is null || chatClient is null)
{
    ConsoleHelper.DisplayError("Failed to initialize the clients.", true);
    return;
}

ConsoleHelper.ShowHeader();

/// <summary>
///     Starts the conversation session.
/// </summary>
using RealtimeSession session =
    await realtimeClient.StartConversationSessionAsync(realtimeModelName);

/// <summary>
///     Configures the conversation session.
/// </summary>
/// <param name="options">The options to configure the session.</param>
session.ConfigureSession(new ConversationSessionOptions()
{
    Voice = ConversationVoice.Echo,
    Tools = { ConversationFunctionToolStatics.WishTool },
    Instructions = PromptStatics.GeneralPrompt,
    InputTranscriptionOptions = new() { Model = "whisper-1" }
});

/// <summary>
///     Prepares the audio output helper.
/// </summary>
AudioOutputHelper audioOutputHelper = new();

/// <summary>
///     Dictionary to track function names by item ID.
/// </summary>
Dictionary<string, string> functionCallTracker = [];

/// <summary>
///     Processes commands received from the service.
/// </summary>
await foreach (RealtimeUpdate update in session.ReceiveUpdatesAsync())
{
    /// <summary>
    ///     Handles the session started update.
    /// </summary>
    if (update is ConversationSessionStartedUpdate)
    {
        ConsoleHelper.DisplayMessage(
            $" <<< Connected: session started", true);

        _ = Task.Run(async () =>
        {
            using AudioInputHelper audioInputHelper =
                AudioInputHelper.Start();

            ConsoleHelper.DisplayMessage(
                $" >>> Listening to microphone input", true);

            await session.SendInputAudioAsync(audioInputHelper);
        });
    }

    /// <summary>
    ///     Handles the speech started update.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.InputSpeechStarted)
    {
        ConsoleHelper.DisplayMessage("", true);
        ConsoleHelper.DisplayMessage(
            $" <<< Start of speech detected",
            true);

        audioOutputHelper.ClearPlayback();
    }

    /// <summary>
    ///     Handles the speech finished update.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.InputSpeechStopped)
    {
        ConsoleHelper.DisplayMessage(
            $" <<< End of speech detected",
            true);
    }

    /// <summary>
    ///     Handles the item streaming started update to capture function name.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.ItemStreamingStarted)
    {
        var startedUpdate = update as dynamic;
        if (startedUpdate.FunctionName != null)
        {
            functionCallTracker[startedUpdate.ItemId] = startedUpdate.FunctionName;
        }
    }

    /// <summary>
    ///     Handles the item streaming audio delta update.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.ItemStreamingPartAudioDelta)
    {
        var deltaUpdate = update as dynamic;
        audioOutputHelper.EnqueueForPlayback(deltaUpdate.AudioBytes);
    }

    /// <summary>
    ///     Handles the item streaming audio transcription delta update.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.ItemStreamingPartAudioTranscriptionDelta)
    {
        var deltaUpdate = update as dynamic;
        ConsoleHelper.DisplayMessage(deltaUpdate.AudioTranscript, false);
    }

    /// <summary>
    ///     Handles the item streaming text delta update.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.ItemStreamingPartTextDelta)
    {
        var deltaUpdate = update as dynamic;
        ConsoleHelper.DisplayMessage(deltaUpdate.Text, false);
    }

    /// <summary>
    ///     Handles the item streaming finished update.
    /// </summary>
    if (update.Kind == RealtimeUpdateKind.ItemStreamingFunctionCallArgumentsFinished)
    {
        var itemFinishedUpdate = update as dynamic;
        ConsoleHelper.DisplayMessage("", true);

        // Retrieve the function name from the tracker
        if (functionCallTracker.TryGetValue(
            itemFinishedUpdate.ItemId, out string? functionName))
        {
            if (functionName == ConversationFunctionToolStatics.WishTool.Name)
            {
                await ConversationFunctionToolStatics.HandleWishToolAsync(
                    chatClient,
                    session,
                    update);
            }

            // Implement other function tools here
            // else if (functionName == OtherTool.Name) { ... }

            // Clean up the tracker
            functionCallTracker.Remove(itemFinishedUpdate.ItemId);
        }
    }

    /// <summary>
    /// Handles the error update.
    /// </summary>
    if (update is RealtimeErrorUpdate errorUpdate)
    {
        ConsoleHelper.DisplayError(
            $" <<< ERROR: {errorUpdate.Message}", true);

        ConsoleHelper.DisplayError(
            errorUpdate.GetRawContent().ToString(), true);

        break;
    }
}
