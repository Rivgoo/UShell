using System.Collections.Generic;
using UShell.Runtime.Core.Commands;

namespace UShell.Runtime.Core.Abstractions
{
	public interface ICommandRegistry
	{
		bool TryGetCommand(string name, out CommandSignature signature);
		IReadOnlyList<CommandSuggestion> GetSuggestions(string input);
		IReadOnlyCollection<CommandSignature> GetAllCommands();
		string GetCompactSignature(CommandSignature signature);
	}
}