using OpenAI.Chat;
using OpenAI.RealtimeConversation;
using SantaClausRealtimeChat.Helpers;
using SantaClausRealtimeChat.Models;
using System.ClientModel;
using System.Text.Json;

namespace SantaClausRealtimeChat.Utils;

/// <summary>
///     Provides static methods and properties for handling conversation 
///     functions related to wishes and emails.
/// </summary>
internal static class ConversationFunctionToolStatics
{
    private const string WishToolDescription =
        "Used whenever a the user asks for wishes for a specific person.";

    /// <summary>
    ///     Represents the tool used to handle wish requests.
    /// </summary>
    public static readonly ConversationFunctionTool WishTool = new()
    {
        Name = nameof(WishTool),
        Description = WishToolDescription,
        Parameters = BinaryData.FromString(@"
                    {
                      ""type"": ""object"",
                      ""properties"": {
                        ""name"": {
                          ""type"": ""string"",
                          ""description"": ""The name of the person to get the wishes from""
                        },
                        ""language"": {
                          ""type"": ""string"",
                          ""description"": ""The current language of the request""
                        }
                      },
                      ""required"": [""name"", ""language""],
                      ""additionalProperties"": false
                    }
                ")
    };

    /// <summary>
    ///     Handles the wish tool functionality asynchronously.
    /// </summary>
    /// <param name="wishes">The list of wishes.</param>
    /// <param name="chatClient">The chat client to use for communication.</param>
    /// <param name="session">The current conversation session.</param>
    /// <param name="itemFinishedUpdate">The update information for 
    /// the finished conversation item.</param>
    public static async Task HandleWishToolAsync(
        ChatClient chatClient,
        RealtimeConversationSession session,
        ConversationItemStreamingFinishedUpdate itemFinishedUpdate)
    {
        ConsoleHelper.DisplayMessage($" <<< Wish Tool invoked -- getting wishes!", true);

        GetWishes(itemFinishedUpdate, out string? name,
            out string? language, out string? wishlist);

        ClientResult<ChatCompletion> result =
            await chatClient.CompleteChatAsync(
                string.Format(
                    PromptStatics.WishPrompt,
                    name, wishlist, language));

        await session.AddItemAsync(
            ConversationItem.CreateFunctionCallOutput(
                itemFinishedUpdate.FunctionCallId,
                result.Value.Content[0].Text));

        await session.StartResponseAsync();
    }    

    /// <summary>
    /// Extracts the wishes, name, and language from the provided update information.
    /// </summary>
    /// <param name="wishes">The list of wishes.</param>
    /// <param name="itemFinishedUpdate">The update information for the 
    /// finished conversation item.</param>
    /// <param name="name">The name of the person to get the wishes from.</param>
    /// <param name="language">The current language of the request.</param>
    /// <param name="wishlist">The extracted wishlist.</param>
    private static void GetWishes(
        ConversationItemStreamingFinishedUpdate itemFinishedUpdate,
        out string? name,
        out string? language,
        out string wishlist)
    {
        // Read the wish items from the database,
        // simulated by a file access
        List<WishItem>? wishes = FileHelper.ReadWishItems();

        using JsonDocument jsonDocument = JsonDocument.Parse(
            itemFinishedUpdate.FunctionCallArguments);

        name = jsonDocument.RootElement.TryGetProperty(
            "name", out JsonElement nameElement)
            ? nameElement.GetString()
            : string.Empty;

        language = jsonDocument.RootElement.TryGetProperty(
            "language", out JsonElement languageElement)
            ? languageElement.GetString()
            : string.Empty;

        string? capturedName = name;

        wishlist = string.Join(", ",
            wishes?.FirstOrDefault(
                x => x.Name.Equals(capturedName,
                StringComparison.InvariantCultureIgnoreCase))?.Wishes ?? []);
    }
}
