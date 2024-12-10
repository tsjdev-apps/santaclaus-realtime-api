using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.RealtimeConversation;
using SantaClausRealtimeChat.Helpers;
using SantaClausRealtimeChat.Utils;
using System.ClientModel;

ConsoleHelper.ShowHeader();

RealtimeConversationClient? realtimeClient = null;
ChatClient? chatClient = null;

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

            string realtimeModelName =
                ConsoleHelper.GetStringFromConsole(
                   "Enter your [yellow]Azure OpenAI[/] realtime model name.");


            AzureOpenAIClient aoaiClient =
                new(
                    new Uri(endpoint),
                    new ApiKeyCredential(apiKey));

            realtimeClient = aoaiClient
                    .GetRealtimeConversationClient(realtimeModelName);

            chatClient = aoaiClient
                    .GetChatClient(chatModelName);
        }
        break;

    case Statics.OpenAI:
        {
            string apiKey =
                ConsoleHelper.GetStringFromConsole(
                    "Enter your [yellow]OpenAI[/] API key.");

            string modelName =
                ConsoleHelper.SelectFromOptions(
                    [Statics.GPT4oMiniKey, Statics.GPT4oKey,
                    Statics.GPT4TurboKey, Statics.GPT4Key],
                    "Enter the [yellow]model name[/] for the chat.");

            OpenAIClient openAIClient = new(apiKey);

            realtimeClient =
                openAIClient.GetRealtimeConversationClient(
                    Statics.OpenAIRealtimeModelName);

            chatClient =
                openAIClient.GetChatClient(modelName);
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
using RealtimeConversationSession session =
    await realtimeClient.StartConversationSessionAsync();

/// <summary>
///     Configures the conversation session.
/// </summary>
/// <param name="options">The options to configure the session.</param>
await session.ConfigureSessionAsync(new ConversationSessionOptions()
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
///     Processes commands received from the service.
/// </summary>
await foreach (ConversationUpdate update in session.ReceiveUpdatesAsync())
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
    if (update is ConversationInputSpeechStartedUpdate speechStartedUpdate)
    {
        ConsoleHelper.DisplayMessage(
            $" <<< Start of speech detected @ {speechStartedUpdate.AudioStartTime}",
            true);

        audioOutputHelper.ClearPlayback();
    }

    /// <summary>
    ///     Handles the speech finished update.
    /// </summary>
    if (update is ConversationInputSpeechFinishedUpdate speechFinishedUpdate)
    {
        ConsoleHelper.DisplayMessage(
            $" <<< End of speech detected @ {speechFinishedUpdate.AudioEndTime}",
            true);
    }

    /// <summary>
    ///     Handles the item streaming part delta update.
    /// </summary>
    if (update is ConversationItemStreamingPartDeltaUpdate deltaUpdate)
    {
        ConsoleHelper.DisplayMessage(deltaUpdate.AudioTranscript, false);
        ConsoleHelper.DisplayMessage(deltaUpdate.Text, false);

        audioOutputHelper.EnqueueForPlayback(deltaUpdate.AudioBytes);
    }

    /// <summary>
    ///     Handles the item streaming finished update.
    /// </summary>
    if (update is ConversationItemStreamingFinishedUpdate itemFinishedUpdate)
    {
        ConsoleHelper.DisplayMessage("", true);

        if (itemFinishedUpdate.FunctionName
            == ConversationFunctionToolStatics.WishTool.Name)
        {
            await ConversationFunctionToolStatics.HandleWishToolAsync(
                chatClient,
                session,
                itemFinishedUpdate);
        }

        // Implement other function tools here
    }

    /// <summary>
    /// Handles the error update.
    /// </summary>
    if (update is ConversationErrorUpdate errorUpdate)
    {
        ConsoleHelper.DisplayError(
            $" <<< ERROR: {errorUpdate.Message}", true);

        ConsoleHelper.DisplayError(
            errorUpdate.GetRawContent().ToString(), true);

        break;
    }
}