#nullable enable
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UShell.Runtime.Unity.Inputs
{
	/// <summary>
	/// A <see cref="MonoBehaviour"/> implementation of <see cref="IInputProvider"/> utilizing 
	/// Unity's New Input System package.
	/// </summary>
	/// <remarks>
	/// Hardcodes the default bindings (Backquote to toggle, Enter to submit, Arrows for history).
	/// </remarks>
	public sealed class InputSystemProvider : MonoBehaviour, IInputProvider
	{
		/// <inheritdoc/>
		public event Action? OnToggleConsole;

		/// <inheritdoc/>
		public event Action? OnSubmit;

		/// <inheritdoc/>
		public event Action? OnHistoryUp;

		/// <inheritdoc/>
		public event Action? OnHistoryDown;

		/// <inheritdoc/>
		public event Action? OnAutocomplete;

		/// <inheritdoc/>
		public event Action? OnEscape;

		private InputAction? _toggleAction;
		private InputActionMap? _uiActionMap;

		private void Awake()
		{
			InitializeToggleAction();
			InitializeUIActionMap();
		}

		private void OnEnable()
		{
			_toggleAction?.Enable();
		}

		private void OnDisable()
		{
			_toggleAction?.Disable();
			_uiActionMap?.Disable();
		}

		private void OnDestroy()
		{
			if (_toggleAction != null)
			{
				_toggleAction.performed -= HandleTogglePerformed;
				_toggleAction.Dispose();
			}

			if (_uiActionMap != null)
			{
				_uiActionMap.Disable();
				_uiActionMap.Dispose();
			}
		}

		/// <inheritdoc/>
		public void SetUIInputActive(bool active)
		{
			if (active)
			{
				_uiActionMap?.Enable();
			}
			else
			{
				_uiActionMap?.Disable();
			}
		}

		private void InitializeToggleAction()
		{
			_toggleAction = new InputAction(
				name: "UShell_ToggleConsole",
				type: InputActionType.Button,
				binding: "<Keyboard>/backquote");

			_toggleAction.performed += HandleTogglePerformed;
		}

		private void InitializeUIActionMap()
		{
			_uiActionMap = new InputActionMap("UShell_UI_Overlay");

			// Submit (Enter / Numpad Enter)
			InputAction submitAction = _uiActionMap.AddAction("Submit", type: InputActionType.Button);
			submitAction.AddBinding("<Keyboard>/enter");
			submitAction.AddBinding("<Keyboard>/numpadEnter");
			submitAction.performed += _ => OnSubmit?.Invoke();

			// History Up (Up Arrow)
			InputAction upAction = _uiActionMap.AddAction("HistoryUp", type: InputActionType.Button, binding: "<Keyboard>/upArrow");
			upAction.performed += _ => OnHistoryUp?.Invoke();

			// History Down (Down Arrow)
			InputAction downAction = _uiActionMap.AddAction("HistoryDown", type: InputActionType.Button, binding: "<Keyboard>/downArrow");
			downAction.performed += _ => OnHistoryDown?.Invoke();

			// Autocomplete (Tab)
			InputAction tabAction = _uiActionMap.AddAction("Autocomplete", type: InputActionType.Button, binding: "<Keyboard>/tab");
			tabAction.performed += _ => OnAutocomplete?.Invoke();

			// Escape / Close
			InputAction escapeAction = _uiActionMap.AddAction("Escape", type: InputActionType.Button, binding: "<Keyboard>/escape");
			escapeAction.performed += _ => OnEscape?.Invoke();
		}

		private void HandleTogglePerformed(InputAction.CallbackContext context)
		{
			OnToggleConsole?.Invoke();
		}
	}
}