using Spectre.Console;

namespace SantaClausRealtimeChat.Helpers;

/// <summary>
///     Provides helper methods for console operations.
/// </summary>
internal static class ConsoleHelper
{
    /// <summary>
    ///     Clears the console and creates the header for the application.
    /// </summary>
    public static void ShowHeader()
    {
        AnsiConsole.Clear();

        Grid grid = new();
        grid.AddColumn();
        grid.AddRow(new FigletText("Santa Claus Phone").Centered().Color(Color.Red));
        grid.AddRow(Align.Center(new Panel("[red]Sample by Thomas Sebastian Jensen ([link]https://www.tsjdev-apps.de[/])[/]")));

        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    ///     Prompts the user to select from a list of options.
    /// </summary>
    /// <param name="options">The list of options to display.</param>
    /// <param name="prompt">The prompt message for the selection.</param>
    /// <returns>The selected option as a string.</returns>
    public static string SelectFromOptions(
        List<string> options,
        string prompt)
    {
        ShowHeader();

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(prompt)
                .AddChoices(options));
    }

    /// <summary>
    ///     Prompts the user to input a valid URL with validation.
    /// </summary>
    /// <param name="prompt">The prompt message for the input.</param>
    /// <returns>The validated URL as a string.</returns>
    public static string GetUrlFromConsole(
        string prompt)
    {
        ShowHeader();

        return AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
                .PromptStyle("white")
                .ValidationErrorMessage("[red]Invalid prompt[/]")
                .Validate(input =>
                {
                    // Check if the input length is within valid range
                    if (input.Length < 3)
                    {
                        return ValidationResult.Error("[red]URL too short[/]");
                    }

                    if (input.Length > 250)
                    {
                        return ValidationResult.Error("[red]URL too long[/]");
                    }

                    // Validate if the input is a proper HTTPS URL
                    if (Uri.TryCreate(input, UriKind.Absolute, out Uri? uri)
                        && uri.Scheme == Uri.UriSchemeHttps)
                    {
                        return ValidationResult.Success();
                    }

                    return ValidationResult.Error("[red]No valid URL[/]");
                }));
    }

    /// <summary>
    ///     Prompts the user to input a string with optional length validation.
    /// </summary>
    /// <param name="prompt">The prompt message for the input.</param>
    /// <param name="validateLength">Whether to validate the length of the input.</param>
    /// <returns>The validated string input.</returns>
    public static string GetStringFromConsole(
        string prompt,
        bool validateLength = true)
    {
        ShowHeader();

        return AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
                .PromptStyle("white")
                .ValidationErrorMessage("[red]Invalid prompt[/]")
                .Validate(input =>
                {
                    // Validate minimum length
                    if (input.Length < 3)
                    {
                        return ValidationResult.Error("[red]Value too short[/]");
                    }

                    // Validate maximum length if enabled
                    if (validateLength && input.Length > 200)
                    {
                        return ValidationResult.Error("[red]Value too long[/]");
                    }

                    return ValidationResult.Success();
                }));
    }


    /// <summary>
    ///     Displays an error message to the console.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="newLine">Indicator if to add a new line.</param>
    public static void DisplayError(string message, bool newLine)
        => WriteToConsole($"[red]{message}[/]", newLine);

    /// <summary>
    ///     Displays a message to the console.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="newLine">Indicator if to add a new line.</param>
    public static void DisplayMessage(string message, bool newLine)
        => WriteToConsole($"[white]{message}[/]", newLine);

    /// <summary>
    ///     Writes the specified text to the console.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="newLine">Indicator if to add a new line.</param>
    private static void WriteToConsole(string text, bool newLine)
    {
        AnsiConsole.Markup(text);

        if (newLine)
        {
            AnsiConsole.WriteLine();
        }
    }
}
