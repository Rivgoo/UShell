#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UShell.Runtime.Core.Abstractions;
using UShell.Runtime.Core.Commands.Fluent;
using UShell.Runtime.Core.Output;
using UShell.Runtime.Core.Output.Formatting;

namespace UShell.Runtime.Unity.BuiltIn
{
	/// <summary>
	/// Provides a suite of commands for manipulating and querying GameObjects in the active scene.
	/// </summary>
	/// <remarks>
	/// These commands rely heavily on the <see cref="UShell.Runtime.Unity.Parsing.Types.GameObjectParser"/> 
	/// and <see cref="UShell.Runtime.Unity.Parsing.Types.Vector3Parser"/> to automatically resolve user strings 
	/// into valid Unity objects and coordinate structures.
	/// </remarks>
	public sealed class GameObjectProfile : ShellProfile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GameObjectProfile"/> class.
		/// </summary>
		public GameObjectProfile(IConsolePrinter printer) : base(printer) { }

		/// <inheritdoc/>
		protected override void Configure(ICommandBuilder builder)
		{
			builder.WithName("go.teleport")
				.WithDescription("Teleports a GameObject to an absolute world position.")
				.WithAlias("tp")
				.AddParameter<GameObject>("target")
				.AddParameter<Vector3>("position")
				.Executes<GameObject, Vector3>(TeleportAbsolute);

			builder.WithName("go.teleport.relative")
				.WithDescription("Moves a GameObject by a relative offset from its current position.")
				.WithAlias("tpr")
				.AddParameter<GameObject>("target")
				.AddParameter<Vector3>("offset")
				.Executes<GameObject, Vector3>(TeleportRelative);

			builder.WithName("go.active")
				.WithDescription("Enables or disables a GameObject in the scene hierarchy.")
				.AddParameter<GameObject>("target")
				.AddParameter<bool>("state")
				.Executes<GameObject, bool>(SetActiveState);

			builder.WithName("go.rotate")
				.WithDescription("Sets the absolute world rotation of a GameObject using Euler angles.")
				.AddParameter<GameObject>("target")
				.AddParameter<Vector3>("eulerRotation")
				.Executes<GameObject, Vector3>(Rotate);

			builder.WithName("go.destroy")
				.WithDescription("Destroys the target GameObject entirely.")
				.AddParameter<GameObject>("target")
				.Executes<GameObject>(DestroyObject);

			builder.WithName("go.info")
				.WithDescription("Prints detailed information about a GameObject, including its components.")
				.AddParameter<GameObject>("target")
				.Executes<GameObject>(PrintObjectInfo);
		}

		private void TeleportAbsolute(GameObject target, Vector3 position)
		{
			target.transform.position = position;
			PrintSuccess($"Teleported '{target.name}' to {FormatVector3(position)}.");
		}

		private void TeleportRelative(GameObject target, Vector3 offset)
		{
			target.transform.position += offset;
			PrintSuccess($"Moved '{target.name}' by offset {FormatVector3(offset)}. New pos: {FormatVector3(target.transform.position)}.");
		}

		private void SetActiveState(GameObject target, bool state)
		{
			target.SetActive(state);
			PrintSuccess($"GameObject '{target.name}' is now {(state ? "Enabled" : "Disabled")}.");
		}

		private void Rotate(GameObject target, Vector3 eulerRotation)
		{
			target.transform.rotation = Quaternion.Euler(eulerRotation);
			PrintSuccess($"Rotated '{target.name}' to {FormatVector3(eulerRotation)}.");
		}

		private void DestroyObject(GameObject target)
		{
			string name = target.name;
			UnityEngine.Object.Destroy(target);
			PrintSuccess($"GameObject '{name}' has been destroyed.");
		}

		private void PrintObjectInfo(GameObject target)
		{
			var sb = new StringBuilder();
			sb.AppendLine(ProfileFormatter.FormatSectionHeader($"gameobject info: {target.name}"));
			sb.AppendLine();

			string activeState = target.activeInHierarchy ? RichText.Color("Active", ShellPalette.Success) : RichText.Color("Inactive", ShellPalette.TextMuted);
			ProfileFormatter.AppendKeyValue(sb, "State", activeState);
			ProfileFormatter.AppendKeyValue(sb, "Tag", RichText.Color(target.tag, ShellPalette.SyntaxValue));
			ProfileFormatter.AppendKeyValue(sb, "Layer", RichText.Color(LayerMask.LayerToName(target.layer), ShellPalette.SyntaxValue));

			sb.AppendLine();
			Transform t = target.transform;
			ProfileFormatter.AppendKeyValue(sb, "Position", FormatVector3(t.position));
			ProfileFormatter.AppendKeyValue(sb, "Rotation", FormatVector3(t.eulerAngles));
			ProfileFormatter.AppendKeyValue(sb, "Scale", FormatVector3(t.localScale));

			Component[] components = target.GetComponents<Component>();

			var headers = new List<string>
			{
				RichText.Bold(RichText.Color("Component Type", ShellPalette.TableHeader)),
				RichText.Bold(RichText.Color("Namespace", ShellPalette.TableHeader))
			};

			var rows = new List<IReadOnlyList<string>>(components.Length);

			foreach (var comp in components)
			{
				if (comp == null) continue; // Can happen with missing scripts
				Type type = comp.GetType();
				rows.Add(new[]
				{
					RichText.Color(type.Name, ShellPalette.SyntaxType),
					RichText.Color(type.Namespace ?? "global", ShellPalette.TextMuted)
				});
			}

			Print(sb.ToString().TrimEnd());
			Print(RichText.Bold(RichText.Color($"\n  ── Components ({rows.Count}) ────────────────────────────────", ShellPalette.HeaderRule)));
			PrintTable(headers, rows, TableStyle.Grid);
		}

		private static string FormatVector3(Vector3 v)
		{
			string formatted = $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
			return RichText.Color(formatted, ShellPalette.SyntaxNumber);
		}
	}
}