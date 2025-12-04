# Use the GPT Realtime (Mini) model to have a conversation with Santa Claus

![header](/docs/header.png)

This repository provides a .NET 10 console application that integrates the GPT Realtime Model. The application demonstrates how to communicate using your voice.

## Features

- **GPT Realtime** or **GPT Realtime Mini**: AI Models to communicate using Text, Images or Audio.
- **Modular Design**: Utilizes helper classes for streamlined code management and readability.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or any compatible IDE
- [Azure.AI.OpenAI](https://www.nuget.org/packages/Azure.AI.OpenAI) NuGet package
- [NAudio](https://www.nuget.org/packages/NAudio) NuGet package
- [Spectre.Console](https://www.nuget.org/packages/Spectre.Console)NuGet package

## Project Strucutre

- `Program.cs`: Entry point of the application.
- `Assets/wishes.json`: Data file containing wishes of several persons.
- `Helpers/AudioInputHelper.cs`: Contains methods for getting the audio from the microphone.
- `Helpers/AudioOutputHelper.cs`: Contains methods to play audio on the default speaker.
- `Helpers/ConsoleHelper.cs`: Contains methods for enhanced console interactions using `Spectre.Console`.
- `Helpers/FileHelper.cs`: Contains methods to read a local file.
- `Models/WishItem.cs`: Model file for a specific *wish item*.
- `Utils/ConversationFunctionToolStatics.cs`: Contains methods for custom *chat tools*.
- `Utils/PromptStatics.cs`: Contains all the prompts used within the application.
- `Utils/Statics.cs`: Provides static constants for various (Azure) OpenAI models and keys.

## Usage

Upon running the application, it will prompt you to enter your credentials. The application will then start listening to the microphone and you are able to interact with *Santa Claus*.

## Application in Action

[![Watch the video](https://img.youtube.com/vi/FLC59eeaBPg/maxresdefault.jpg)](https://youtu.be/FLC59eeaBPg)

## Blog Posts

If you are more interested into details, please see the following posts on [medium.com](https://medium.com/@tsjdevapps) or in my [personal blog](https://www.tsjdev-apps.de):

- [Use the GPT-4o Realtime model to have a conversation with Santa Claus](https://medium.com/medialesson/use-the-gpt-4o-realtime-model-to-have-a-conversation-with-santa-claus-08dd3ac4d97b)
- [Einrichtung von OpenAI](https://www.tsjdev-apps.de/einrichtung-von-openai/)
- [Einrichtung von Azure OpenAI](https://www.tsjdev-apps.de/einrichtung-von-azure-openai/)
