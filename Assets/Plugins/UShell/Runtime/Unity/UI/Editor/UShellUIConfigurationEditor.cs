#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UShell.Runtime.Unity.UI.Configuration;

namespace UShell.Editor.UI
{
	/// <summary>
	/// Custom Unity Inspector layout for the <see cref="UShellUIConfiguration"/> asset.
	/// </summary>
	[CustomEditor(typeof(UShellUIConfiguration))]
	public sealed class UShellUIConfigurationEditor : UnityEditor.Editor
	{
		/// <inheritdoc/>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space(5);

			// --- General Settings ---
			DrawHeader("General Settings");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxLogs"));

			// --- Typography ---
			DrawHeader("Typography");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_mainFont"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_inputFontSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_logFontSize"));

			// --- Global Monospace ---
			DrawHeader("Global Monospace (ASCII Alignment)");
			SerializedProperty forceMonospaceProp = serializedObject.FindProperty("_forceGlobalMonospace");
			EditorGUILayout.PropertyField(forceMonospaceProp);

			if (forceMonospaceProp.boolValue)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_globalMonospaceWidth"));
				EditorGUI.indentLevel--;
			}

			// --- Suggestions ---
			DrawHeader("Suggestions (Info Blocks)");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_suggestionBackgroundColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_suggestionPaddingX"));

			// --- Colors & Themes ---
			DrawHeader("Interface Colors");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_ghostTextColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_promptColor"));

			// --- Log State Colors ---
			DrawHeader("Log State Colors");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_standardLogColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_successLogColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_warningLogColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_errorLogColor"));

			// --- Icons ---
			DrawHeader("Icons");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_standardIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_successIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_warningIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_errorIcon"));

			EditorGUILayout.Space(10);

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Draws a consistently spaced and styled header with a subtle separator line.
		/// </summary>
		private static void DrawHeader(string title)
		{
			EditorGUILayout.Space(15);
			EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

			// Draw a soft separator line
			Rect rect = EditorGUILayout.GetControlRect(false, 2f);
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));

			EditorGUILayout.Space(5);
		}
	}
}
#endif