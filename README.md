<p align="center">
  <img src="Assets/Plugins/UShell/Graphics/Editor/UShell_Icon.png" alt="UShell Logo" width="128"/>
</p>

<h1 align="center">UShell</h1>

<p align="center">
  <b>A High-Performance, Modular In-Game Developer Console for Unity.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2021.3%2B-lightgrey.svg" alt="Unity 2021.3+">
  <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License: MIT">
  <img src="https://img.shields.io/badge/PRs-Welcome-brightgreen.svg" alt="PRs Welcome">
</p>

---

## 📖 About UShell

**UShell** is a robust and highly customizable in-game developer console designed to help you debug, manage, and interact with your Unity game at runtime. 

Unlike many other console assets that rely on slow reflection and hidden magic, UShell is built on strict software architecture principles (SOLID). It provides a clean, fast, and safe way to execute commands, view logs, and monitor game state without bloating your production builds.

Whether you need to give the player items, teleport across the map, or check memory allocation in a standalone build, UShell gives you the tools to do it elegantly.

## ✨ Key Features

*   **Fluent API (No Reflection):** Commands are registered explicitly via a clean, chainable API. No assembly scanning, no hidden `[Attributes]`, and zero startup overhead.
*   **High Performance:** The lexical and syntax parser is built using `ReadOnlySpan<char>`, `ref struct`, and `ArrayPool<T>`. This ensures a zero-allocation parsing phase, keeping the Garbage Collector completely silent during command execution.
*   **Smart Autocomplete:** Features a Trie-based real-time autocomplete engine. It instantly suggests commands, shows parameter signatures, and warns about type mismatches *before* you press Enter.
*   **Clean Architecture:** The core logic (`UShell.Core`) has **zero dependencies on Unity**. It strictly separates parsing, command registry, and execution from the UI and engine layer.
*   **Type Safety & Extensibility:** Comes with built-in parsers for standard types (`int`, `float`, `bool`) and Unity types (`Vector3`, `Color`, `GameObject`). Easily register your own parsers for custom game entities.
*   **Environment Contexts:** Restrict specific cheat commands strictly to the `Editor` or `Development` builds, ensuring they never leak into your `Release` game.

---

## 🚀 Installation

### Option 1: Unity Package Manager (UPM)
1. Open Unity and go to `Window > Package Manager`.
2. Click the `+` icon in the top left corner and select **"Add package from git URL..."**.
3. Enter the URL of this repository:
   ```text
   https://github.com/YOUR_USERNAME/UShell.git?path=/UShell/Assets/Plugins/UShell
   ```
4. Click **Add**.

### Option 2: Manual Install
Download the latest release and extract the `UShell` folder directly into your project's `Assets/Plugins/` directory.

---

## 🛠️ Quick Start

Integrating UShell into your game is incredibly straightforward. You define your commands in a **Profile**, and then register that profile with the **ShellBuilder**.

### 1. Create a Command Profile
Group related commands into a profile. Here, you can easily inject your game's systems using standard Dependency Injection.

```csharp
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;

public class PlayerDebugProfile : ShellProfile
{
    private readonly IPlayerService _playerService;

    // Inject your game services through the constructor
    public PlayerDebugProfile(IConsolePrinter printer, IPlayerService playerService) : base(printer)
    {
        _playerService = playerService;
    }

    protected override void Configure(ICommandBuilder builder)
    {
        builder.WithName("player.heal")
            .WithDescription("Restores the player's health to maximum.")
            .WithAlias("heal")
            .Executes(() => 
            {
                _playerService.HealFull();
                PrintSuccess("Player health restored!");
            });

        builder.WithName("player.give")
            .WithDescription("Gives a specific amount of an item to the player.")
            .AddParameter<string>("itemId")
            .AddOptionalParameter<int>("amount", 1) // Default value is 1
            .Executes<string, int>((id, amount) => 
            {
                _playerService.GiveItem(id, amount);
                Print($"Gave {amount}x {id} to the player.");
            });
    }
}
```

### 2. Bootstrap the Console
In your game's entry point (or bootstrap script), build the console core and pass it to the UI controller.

```csharp
using UnityEngine;
using UShell.Runtime.Core.Bootstrapping;
using UShell.Runtime.Core.Commands;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private UShell.Runtime.Unity.Output.UnityConsolePrinter _printer;

    private void Start()
    {
        // 1. Initialize your game services
        IPlayerService myPlayerService = new PlayerService();

        // 2. Build the UShell Core
        var shellCore = new ShellBuilder(EnvironmentTag.Development)
            .AddProfile(new PlayerDebugProfile(_printer, myPlayerService))
            .Build();
            
        // The console is now ready! 
        // (If using the provided UShellManager prefab, it handles UI setup for you).
    }
}
```

---

## 🧠 Advanced Usage

### Custom Type Parsers
Want to pass your own `ItemType` enum or a custom `PlayerId` struct directly in the console? Just create a type parser!

```csharp
public class ItemTypeParser : TypeParser<ItemType>
{
    public override ExecutionResult<ItemType> ParseTyped(string input)
    {
        if (Enum.TryParse(input, true, out ItemType result))
        {
            return ExecutionResult<ItemType>.Success(result);
        }

        return ExecutionResult<ItemType>.Failure(
            ShellError.Create(ShellErrorCode.Bind_TypeMismatch, -1, input, "ItemType"));
    }
}

// Then register it during setup:
// new ShellBuilder().AddTypeParser(new ItemTypeParser())...
```

### Environment Restrictions
Prevent dangerous commands from shipping to players by tagging them.

```csharp
builder.WithName("kill_all_enemies")
    .RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
    .Executes(KillEnemies);
```

---

## 🤝 Contributing

This project is open-source, and community contributions are highly appreciated! 

If you want to contribute, please follow these guidelines:
1. **Maintain Clean Architecture:** Do not add Unity-specific code (`UnityEngine`) inside the `UShell.Core` assembly.
2. **Zero Allocations:** When modifying the parser or lexer, avoid allocating new reference types. Use `ReadOnlySpan<char>` and object pooling.
3. **Small PRs:** Keep Pull Requests focused on a single feature or bug fix.

To get started:
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/amazing-feature`).
3. Commit your changes (`git commit -m 'Add amazing feature'`).
4. Push to the branch (`git push origin feature/amazing-feature`).
5. Open a Pull Request.

---

## 📄 License

This project is licensed under the **MIT License**.

You are free to use, modify, and distribute this software in your personal or commercial projects without any restrictions. The intellectual property of the core architecture remains with the original author. If you build upon this, a credit link to this repository is appreciated but not mandatory.

See the [LICENSE](LICENSE) file for more details.