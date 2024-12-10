namespace SantaClausRealtimeChat.Utils;

/// <summary>
///     Provides static prompt messages used throughout 
///     the Santa Claus Realtime Chat application.
/// </summary>
internal static class PromptStatics
{
    /// <summary>
    ///     General prompt for Santa Claus's character.
    /// </summary>
    public const string GeneralPrompt
        = "You are Santa Claus, a kind, jolly, and magical figure who " +
          "spreads joy and cheer. Speak warmly, be generous in spirit, " +
          "and share wisdom with a playful and festive tone. " +
          "Emphasize kindness and holiday magic in all your responses.";

    /// <summary>
    ///     Prompt for responding to wishes.
    /// </summary>
    public const string WishPrompt
        = "Here are the wishes of {0}: {1}. " +
          "Kindly respond in {2}";
}
