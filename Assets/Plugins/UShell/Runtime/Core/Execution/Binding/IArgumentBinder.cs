#nullable enable
using System.Collections.Generic;
using UShell.Runtime.Core.Commands;
using UShell.Runtime.Core.Parsing.Syntax;

namespace UShell.Runtime.Core.Execution.Binding
{
	public interface IArgumentBinder
	{
		ExecutionResult<object?[]> BindArguments(IReadOnlyList<CommandParameter> parameters, IReadOnlyList<SyntaxNode> arguments);
	}
}