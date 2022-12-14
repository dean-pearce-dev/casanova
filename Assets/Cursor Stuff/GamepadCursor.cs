using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class GamepadCursor : MonoBehaviour
{
	[SerializeField]
	private PlayerInput playerInput;
	private Mouse virtualMouse;

	private Mouse currentMouse;

	[SerializeField] RectTransform controllerCursorTransform;

	[SerializeField]
	float cursorSpeed = 1000f;

	private bool previousMouseState;

	[SerializeField]
	private RectTransform canvasRectTransform;

	[SerializeField]
	private float padding = 35f;

	private string previousControlScheme = "";

	private void OnEnable()
	{

		currentMouse = Mouse.current;
		if (virtualMouse == null )
		{
			virtualMouse = (Mouse) InputSystem.AddDevice( "VirtualMouse" );
		}
		else if ( !virtualMouse.added )
		{
			InputSystem.AddDevice( virtualMouse );
		}

		InputUser.PerformPairingWithDevice( virtualMouse, playerInput.user );

		if (controllerCursorTransform != null )
		{
			Vector2 position = controllerCursorTransform.anchoredPosition;
			InputState.Change( virtualMouse.position, position );
		}

		InputSystem.onAfterUpdate += UpdateMotion;
		playerInput.onControlsChanged += OnControlsChanged;
	}

	private void OnDisable()
	{
		if (virtualMouse != null && virtualMouse.added)
		{ 
			InputSystem.RemoveDevice( virtualMouse );
		}
		InputSystem.onAfterUpdate -= UpdateMotion;
		playerInput.onControlsChanged -= OnControlsChanged;
	}

	private void UpdateMotion()
	{
		if( Settings.g_inMenu )
		{

			if ( virtualMouse == null || Gamepad.current == null )
			{
				return;
			}

			Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
			deltaValue *= cursorSpeed * Time.unscaledDeltaTime;

			Vector2 currentPosition = virtualMouse.position.ReadValue();
			Vector2 newPosition = currentPosition + deltaValue;

			newPosition.x = Mathf.Clamp( newPosition.x, padding, Screen.width - padding );
			newPosition.y = Mathf.Clamp( newPosition.y, padding, Screen.height - padding );

			InputState.Change( virtualMouse.position, newPosition );
			InputState.Change( virtualMouse.delta, deltaValue );

			bool aButtonIsPressed = Gamepad.current.aButton.IsPressed();

			if ( previousMouseState != aButtonIsPressed )
			{
				virtualMouse.CopyState<MouseState>( out var mouseState );
				mouseState.WithButton( MouseButton.Left, aButtonIsPressed );
				InputState.Change( virtualMouse, mouseState );
				previousMouseState = aButtonIsPressed;
			}

			AnchorCursor( newPosition );
		}

	}
	private void AnchorCursor( Vector2 position )
	{
		Vector2 anchoredPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, position, null, out anchoredPosition);

		controllerCursorTransform.anchoredPosition = anchoredPosition;
	}

	private void OnControlsChanged( PlayerInput input)
	{
		if ( playerInput.currentControlScheme == Settings.g_mouseScheme && previousControlScheme != Settings.g_mouseScheme )
		{
			if ( Settings.g_inMenu )
			{
				currentMouse.WarpCursorPosition( virtualMouse.position.ReadValue() );
				controllerCursorTransform.gameObject.SetActive( false );
				Cursor.visible = true;
			}
			previousControlScheme = Settings.g_mouseScheme;
		}
		else if ( playerInput.currentControlScheme == Settings.g_gamepadScheme && previousControlScheme != Settings.g_gamepadScheme )
		{
			if ( Settings.g_inMenu )
			{
				InputState.Change( virtualMouse.position, currentMouse.position.ReadValue() );
				AnchorCursor( currentMouse.position.ReadValue() );
				controllerCursorTransform.gameObject.SetActive( true );
				Cursor.visible = false;
			}
			previousControlScheme = Settings.g_gamepadScheme;
		}

		Settings.g_currentControlScheme = previousControlScheme;
	}
	/*
	private void Update()
	{
		if (previousControlScheme != playerInput.currentControlScheme )
		{
			OnControlsChanged(playerInput);
		}
		previousControlScheme = playerInput.currentControlScheme;
	}*/
}
