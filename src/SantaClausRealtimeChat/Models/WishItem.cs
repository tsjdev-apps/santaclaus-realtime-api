namespace SantaClausRealtimeChat.Models;

/// <summary>
///     Represents an item on a wish list with a name and an array of wishes.
/// </summary>
/// <param name="Name">The name of the wish item.</param>
/// <param name="Wishes">An array of wishes associated with the wish item.</param>
internal record class WishItem(string Name, string[] Wishes);
