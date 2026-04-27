<p align="center">
  <img src="Assets/Plugins/UShell/Graphics/Editor/UShell_Icon.png" alt="UShell Logo" width="128"/>
</p>

<h1 align="center">UShell</h1>

<p align="center">
  <b>A high-performance, reflection-free, and architecturally clean developer console for Unity. </b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2021.3%2B-lightgrey.svg" alt="Unity 2021.3+">
  <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License: MIT">
  <img src="https://img.shields.io/badge/PRs-Welcome-brightgreen.svg" alt="PRs Welcome">
</p>

---

**UShell** is built for developers who demand robust tooling. It abandons the traditional (and costly) approach of scanning assemblies via reflection at startup. Instead, it relies on a fluent, statically-typed registration API.

## 🏛️ Architecture & Core Philosophy

Understanding UShell's architecture helps you leverage its full potential:

1. **Zero-Reflection / Explicit Registration:** UShell does not use `[ConsoleCommand]` attributes. Instead, you construct `CommandSignatures` using a Builder pattern (`ShellBuilder`). This guarantees predictable memory usage and zero CPU spikes during game initialization.
2. **The Execution Pipeline:** 
   * **Lexing & Parsing:** User input (e.g., `spawn -count 5 "Orc"`) is tokenized and parsed into an Abstract Syntax Tree (AST).
   * **Binding:** The AST nodes are mapped to your command's required C# types via `ITypeParser<T>` implementations.
   * **Invocation:** The execution delegate is invoked with strongly-typed arguments.
3. **Dependency Inversion:** The core engine (`IShellCore`) knows absolutely nothing about Unity's UI (Canvas, TextMeshPro) or Input systems (New Input System, Rewired). It communicates with the outside world purely through abstractions (`IConsolePrinter`, `IInputProvider`).
4. **Interactive Execution:** Unlike traditional consoles that just "fire and forget", UShell commands can suspend their execution to await user text input, confirm Y/N prompts, or update live progress bars without freezing the main Unity thread.

---

## 📦 1. Installation & Quick Start

**Requirements:** Unity 2021.3+ | TextMeshPro | Unity New Input System.

### Option 1: Unity Package (Recommended)
1.  Go to the [**Releases**](../../releases) page of this repository.
2.  Download the latest `UShell-vX.X.X.unitypackage`.
3.  Drag and drop the downloaded file into your Unity Project window.
4.  Follow the import settings and ensure the `Assets/Plugins/UShell` directory is selected.

### Option 2: Unity Package Manager (Git URL)
If you prefer to keep the console isolated from your `Assets` folder:
1.  Open the **Package Manager** in Unity (`Window` > `Package Manager`).
2.  Click the **+** icon and select **Add package from git URL...**
3.  Enter the following URL:
    `https://github.com/Rivgoo/UShell.git?path=/Assets/Plugins/UShell`

### Option 3: Manual
1.  Clone this repository or download the source code.
2.  Copy the `Assets/Plugins/UShell` folder into your project's `Assets` directory.

---

### ⚙️ Prerequisites Check
Regardless of the installation method, ensure these steps are completed:
*   **TextMeshPro:** Ensure you have TextMeshPro Essential Resources imported (`Window` > `TextMeshPro` > `Import TMP Essential Resources`).
*   **Input System:** UShell uses the New Input System by default. Ensure it is enabled in your `Project Settings` > `Player` > `Active Input Handling`.

### Your First Custom Command
To add commands to the bootstrapper, create a `MonoBehaviour` implementing `IShellConfigurator`:

```csharp
using UnityEngine;
using UShell.Runtime.Unity.Bootstrapping;
using UShell.Runtime.Core.Bootstrapping;

public class GameShellConfigurator : MonoBehaviour, IShellConfigurator
{
    public void Configure(ShellBuilder builder, ShellBootstrapContext context)
    {
        // Add a custom profile. We pass the printer so the profile can output text.
        builder.AddProfile(new PlayerCheatsProfile(context.Printer));
    }
}
```

---

## 🛠️ 2. Profiles & The Fluent API (Deep Dive)

Commands in UShell are grouped into logical collections called **Profiles**. To create one, inherit from `ShellProfile` (which gives you helper methods like `Print`, `PrintWarning`, etc.) and implement `Configure(ICommandBuilder builder)`.

### The `ICommandBuilder` API
The entry point is `builder.WithName("command.name")`. 
*   **Rule:** Names cannot contain spaces, quotes, brackets, or commas.
*   **Returns:** An `ICommandConfigurator` instance to chain the rest of your setup.

### The `ICommandConfigurator` API

#### 1. Metadata Methods
*   `.WithDescription(string desc)`: Sets the human-readable summary shown when the user types `help`.
*   `.WithAlias(string alias)`: Adds an alternative name that resolves to this command (e.g., `rm` for `remove`). You can call this multiple times.
*   `.RestrictedTo(EnvironmentTag tags)`: A bitmask (`Editor`, `Development`, `Release`, `Any`) defining where this command exists. Useful for stripping cheat commands from production builds automatically.

#### 2. Parameter Binding
Order matters! Positional arguments passed by the user will be bound in the exact order you declare them here.
*   `.AddParameter<T>(string name)`: Declares a required parameter of type `T`. The `name` is used for named arguments (e.g., `-amount 50`). `T` must have a registered `ITypeParser`.
*   `.AddOptionalParameter<T>(string name, T defaultValue)`: Declares an optional parameter. If the user omits it, `defaultValue` is injected. **Rule:** Optional parameters must be declared *after* all required parameters.

#### 3. Autocomplete Suggestions
UShell provides an IDE-like fuzzy search. You can attach suggestions to the **last added parameter**.
*   `.WithSuggestions(IEnumerable<string> staticList)`: Provides a fixed list of suggestions.
    ```csharp
    .AddParameter<string>("weather")
    .WithSuggestions(new[] { "clear", "rain", "storm" })
    ```
*   `.WithSuggestions(Func<SuggestionContext, IEnumerable<string>> provider)`: Dynamic resolution. The `SuggestionContext` contains the partial text the user has typed so far.
    ```csharp
    .AddParameter<string>("playerName")
    .WithSuggestions(ctx => NetworkManager.GetActivePlayers()
                                          .Where(p => p.StartsWith(ctx.PartialValue)))
    ```
*   `.WithSuggestions(ISuggestionProvider provider)`: For complex, reusable suggestion logic encapsulated in a dedicated class.

*Note: Enums and Booleans automatically generate their own suggestions. You don't need to manually configure them.*

#### 4. Execution Delegates
Every command configuration chain **must end** with an `Executes...` method to bind the actual C# logic. The arity (number of arguments) and types of the delegate *must perfectly match* the parameters you added.

*   **Synchronous Execution:**
    *   `.Executes(Action action)`
    *   `.Executes<T1, T2...>(Action<T1, T2...> action)` (Up to 5 parameters)
    *   `.ExecutesReturning<TResult>(Func<TResult> func)`: The returned value will be automatically printed to the console as a success log.
    *   `.ExecutesReturning<T1, TResult>(Func<T1, TResult> func)`

*   **Asynchronous Execution:**
    *   `.ExecutesAsync(Func<Task> action)`: Awaits the task. If it throws, the exception is caught and printed gracefully.

#### Complete Profile Example:
```csharp
public class ItemProfile : ShellProfile
{
    public ItemProfile(IConsolePrinter printer) : base(printer) { }

    protected override void Configure(ICommandBuilder builder)
    {
        builder.WithName("item.give")
            .WithDescription("Adds an item to the local player's inventory.")
            .WithAlias("give")
            .RestrictedTo(EnvironmentTag.Editor | EnvironmentTag.Development)
            
            .AddParameter<string>("itemId")
            .WithSuggestions(GetItemDatabaseIds) // Dynamic suggestions
            
            .AddOptionalParameter<int>("amount", 1)
            .AddOptionalParameter<bool>("equipImmediately", false)
            
            // The signature of GiveItem MUST be (string, int, bool)
            .Executes<string, int, bool>(GiveItem);
    }

    private void GiveItem(string id, int amount, bool equip)
    {
        PrintSuccess($"Gave {amount}x '{id}'. Equipped: {equip}");
    }

    private IEnumerable<string> GetItemDatabaseIds(SuggestionContext ctx) => new[] { "sword", "shield", "potion" };
}
```

---

## ⏳ 3. Interactive Commands (`ICommandContext`)

Sometimes a command takes a long time, or requires the user to confirm a destructive action. UShell allows commands to "lock" the console and interact with the user via `ICommandContext`.

**Rules for Interactive Commands:**
1. You must call `.WithTimeout(TimeSpan limit)` before execution.
2. You must bind using `.ExecutesInteractiveAsync(...)`.
3. The first argument of your delegate must be `ICommandContext`.

### The `ICommandContext` API
*   `CancellationToken Token`: Triggered if the timeout is reached, or if the user presses `Escape` to abort. Pass this to your game's async tasks.
*   `Task<bool> ConfirmAsync(string msg)`: Pauses execution, prompts the user with `(y/n)`, and awaits their response.
*   `Task<string> PromptAsync(string msg)`: Pauses execution and waits for the user to type a string.
*   `IProgressReporter CreateProgressBar(string taskName)`: Spawns a visual progress bar.
*   `Print(...)`, `PrintSuccess(...)`, etc.: Thread-safe methods to push logs while the task is running.

### Interactive Example:
```csharp
builder.WithName("db.wipe")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .ExecutesInteractiveAsync(WipeDatabaseAsync);

private async Task WipeDatabaseAsync(ICommandContext ctx)
{
    // 1. Ask for confirmation
    bool confirmed = await ctx.ConfirmAsync("Wipe the entire database?");
    if (!confirmed) 
    {
        ctx.PrintWarning("Aborted by user.");
        return; 
    }

    // 2. Spawn a progress bar (use 'using' so it auto-completes to 100% when disposed)
    using (IProgressReporter progress = ctx.CreateProgressBar("Deleting..."))
    {
        for (int i = 0; i <= 100; i += 10)
        {
            ctx.Token.ThrowIfCancellationRequested(); // Abort if user hits Escape
            progress.Report(i / 100f, $"{i}% complete");
            await Task.Delay(250, ctx.Token); // Fake work
        }
    }
    
    ctx.PrintSuccess("Database wiped successfully.");
}
```

---

## 🔠 4. Custom Type Parsers (`ITypeParser<T>`)

UShell's Argument Binder converts strings into C# objects. Out of the box, it supports `int`, `float`, `bool`, `string`, `Vector2/3/4`, `Quaternion`, `Color`, and `GameObject` (via `GameObject.Find`).

You can teach UShell to understand your game's custom types by creating a `TypeParser<T>`. 

### Building a Custom Parser
Inherit from `TypeParser<T>` and implement `ParseTyped(string input)`. 
Return `ExecutionResult<T>.Success` or `ExecutionResult<T>.Failure`. Use `ShellError.Create` for failures to guarantee proper UI visualization.

```csharp
using UShell.Runtime.Core.Execution;
using UShell.Runtime.Core.Diagnostics;
using UShell.Runtime.Core.Parsing.Types;

// Let's parse a custom struct: public struct PlayerId { public int Id; }

public class PlayerIdParser : TypeParser<PlayerId>
{
    public override ExecutionResult<PlayerId> ParseTyped(string input)
    {
        if (int.TryParse(input, out int id))
        {
            // Verify it exists in your game
            if (PlayerManager.PlayerExists(id))
            {
                return ExecutionResult<PlayerId>.Success(new PlayerId { Id = id });
            }
            return ExecutionResult<PlayerId>.Failure(
                ShellError.Create(ShellErrorCode.Bind_CustomError, -1, $"Player with ID {id} not found."));
        }

        return ExecutionResult<PlayerId>.Failure(
            ShellError.Create(ShellErrorCode.Bind_CustomError, -1, $"'{input}' is not a valid integer."));
    }
}
```

### Registering the Parser
In your `IShellConfigurator` or manual bootstrapping setup:
```csharp
builder.AddTypeParser(new PlayerIdParser());
```
Now, you can seamlessly use `.AddParameter<PlayerId>("player")` in your profiles.

---

## 💾 5. Session State & Macros

The `ISessionState` acts as a runtime memory store. Users can assign command outputs or literals to variables using the `$` syntax.

**In the console UI:**
```bash
> $speed = 5.5
> $boss = spawn "Dragon"
> set_speed $boss $speed
```

**Accessing Session State from C#:**
The `ISessionState` is injected into `ShellBootstrapContext` and can be passed to your profiles.
*   `TryGetValue(string name, out object value)`: Retrieve a stored macro.
*   `SetValue(string name, object value)`: Programmatically set a macro.
*   `Clear()`: Wipes all memory.

Built-in commands `macro.list`, `macro.delete`, and `macro.clear` manage this state.

---

## 🎨 6. Formatting & Visuals (`ShellProfile` & `ShellPalette`)

UShell prioritizes beautiful, readable output. Inside any `ShellProfile`, use the built-in protected methods instead of `Debug.Log`.

### Output Methods
*   `Print(string)`: Standard white/grey text.
*   `PrintSuccess(string)`: Green text.
*   `PrintWarning(string)`: Amber/Yellow text.
*   `PrintError(string)`: Red text.
*   `PrintList(string title, IReadOnlyList<string> items, int limit)`: Formats a numbered list under a section header, truncating if it exceeds `limit`.
*   `PrintTable(headers, rows, TableStyle)`: Automatically calculates column widths and draws an ASCII table.

### `ShellPalette` & `RichText`
Never hardcode hex colors. Use `ShellPalette` constants to ensure your custom outputs match the console's theme. Combine them with the `RichText` utility.

```csharp
string entityName = RichText.Color("Dragon", ShellPalette.SyntaxValue);
string hpText = RichText.Bold(RichText.Color("5000", ShellPalette.SyntaxNumber));

Print($"Spawned {entityName} with {hpText} HP.");
```

### `ProfileFormatter`
A utility class with standard layout helpers:
*   `ProfileFormatter.FormatSectionHeader("my title")`: Generates `── my title ───`
*   `ProfileFormatter.AppendKeyValue(StringBuilder, key, value)`: Aligns key-value pairs cleanly.

---

## ⚙️ 7. Manual Bootstrapping (Zenject / VContainer)

If you don't want to use the `ComponentShellBootstrapper`, you can wire up UShell entirely in code using `CoreShellBootstrapper`. This is ideal for pure DI architectures.

```csharp
public class TerminalInstaller : MonoInstaller
{
    [SerializeField] private UShellManager _manager; // The DontDestroyOnLoad root
    
    public override void InstallBindings()
    {
        // 1. Create the bootstrapper
        var bootstrapper = new CoreShellBootstrapper(EnvironmentTag.Development, mirrorLogsToUnityConsole: true);
        
        // 2. Add dependencies
        bootstrapper.AddTypeParser(new PlayerIdParser());
        bootstrapper.AddProfile(context => new NetworkAdminProfile(context.Printer, Container.Resolve<INetworkService>()));
        
        // 3. Build the core architecture
        BootstrapResult result = bootstrapper.Build();
        
        // 4. Initialize the Unity Manager wrapper
        _manager.Initialize(result);
    }
}
```

---

## 🖥️ 8. UI & Custom Input Providers

### UI Configuration
The visual appearance of UShell is controlled by a ScriptableObject: `UShell_UI_Configuration`.
*   **Typography:** Assign custom `TMP_FontAsset` and tweak font sizes.
*   **Colors:** Change prompt colors, background suggestion colors, and standard log colors.
*   **Monospace Override:** If your font isn't monospaced, ASCII tables will misalign. Enable `ForceGlobalMonospace` and set the `em` width (usually `0.6`) to force TextMeshPro to align characters perfectly.

### Custom Input (`IInputProvider`)
UShell uses Unity's New Input System by default (`InputSystemProvider.cs`). If you use Rewired or the legacy Input Manager, write a class that implements `IInputProvider` and attach it to the `UShellManager` GameObject. UShell will automatically detect and use it.

```csharp
public interface IInputProvider
{
    event Action OnToggleConsole; // Toggle UI visibility (e.g., Tilde key)
    event Action OnSubmit;        // Enter command (e.g., Enter key)
    event Action OnHistoryUp;     // E.g., Up Arrow
    event Action OnHistoryDown;   // E.g., Down Arrow
    event Action OnAutocomplete;  // E.g., Tab key
    event Action OnEscape;        // Abort interactive tasks / close console
    
    // UShell will call this to disable hotkeys when the console is closed
    void SetUIInputActive(bool active); 
}
```

---

## 📦 9. Built-in Profiles Summary
*   **`ConsoleManagementProfile`**: Core commands like `help`, `clear`, `history`, `alias`, and macro controls.
*   **`EnvironmentInfoProfile`**: System commands like `info`, `env`, `platform`, `game.version`.
*   **`MathUtilityProfile`**: Includes `eval` (advanced math expression evaluator), `random`, and `convert` (metric/imperial, hex/bin/dec conversions).
*   **`RuntimeDiagnosticsProfile`**: Editor/Dev performance tools like `stats` (FPS, Frame Time), `mem` (GC and Heap allocations), `objects`, `layer.find`, and `prefs.clear`.

---

## 📄 License
This project is licensed under the **MIT License**.
You are free to use, modify, and distribute this software in your personal or commercial projects without any restrictions. The intellectual property of the core architecture remains with the original author. If you build upon this, a credit link to this repository is appreciated but not mandatory.

See the [LICENSE](LICENSE) file for more details.