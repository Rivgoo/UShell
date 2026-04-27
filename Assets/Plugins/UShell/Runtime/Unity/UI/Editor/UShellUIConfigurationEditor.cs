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

			EditorGUILayout.Space(10);
			DrawHeader("Typography Settings");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_mainFont"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_inputFontSize"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_logFontSize"));

			EditorGUILayout.Space(10);
			DrawHeader("Global Monospace (ASCII Alignment)");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_forceGlobalMonospace"));

			if (serializedObject.FindProperty("_forceGlobalMonospace").boolValue)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_globalMonospaceWidth"));
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space(10);
			DrawHeader("Suggestions (Info Blocks)");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_suggestionBackgroundColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_suggestionPaddingX"));

			EditorGUILayout.Space(10);
			DrawHeader("Colors & Themes");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_ghostTextColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_promptColor"));

			EditorGUILayout.Space(10);
			DrawHeader("Log State Colors");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_standardLogColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_successLogColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_warningLogColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_errorLogColor"));

			EditorGUILayout.Space(10);
			DrawHeader("Icons");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_standardIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_successIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_warningIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_errorIcon"));

			EditorGUILayout.Space(10);
			DrawHeader("Limits");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxLogs"));

			serializedObject.ApplyModifiedProperties();
		}

		private static void DrawHeader(string title)
		{
			EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
			Rect rect = EditorGUILayout.GetControlRect(false, 2f);
			EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1f));
			EditorGUILayout.Space(5);
		}
	}
}
#endif