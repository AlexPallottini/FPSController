using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPController
{
    public class PlayerControlInputs : MonoBehaviour
    {
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Jump { get; private set; }
        public bool Sprint { get; private set; }
        public bool Crouch { get; private set; }
        public bool Zoom { get; private set; }
        public event Action<bool> OnZoomChanged;
        public bool Interact { get; private set; }
        public event Action<bool> OnInteractChanged;

        [field:Header("Mouse Cursor Settings")]
        [field: SerializeField, Tooltip("Whether or not the cursor will be locked inside the screen for the game")] public bool CursorLocked { get; private set; } = true;
        [field: SerializeField, Tooltip("Whether or not to use the cursor as an input for the movement of the camera")] public bool CursorInputForLook { get; private set; } = true;

        private void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }
        private void OnLook(InputValue value)
        {
            if (CursorInputForLook) Look = value.Get<Vector2>();
        }
        private void OnJump(InputValue value)
        {
            Jump = value.isPressed;
        }
        private void OnSprint(InputValue value)
        {
            Sprint = value.isPressed;
        }
        private void OnCrouch(InputValue value)
        {
            Crouch = value.isPressed;
        }
        private void OnZoom(InputValue value)
        {
            Zoom = value.isPressed;
            OnZoomChanged?.Invoke(value.isPressed);
        }
        private void OnInteract(InputValue value)
        {
            Interact = value.isPressed;
            OnInteractChanged?.Invoke(value.isPressed);
        }
        private void OnApplicationFocus(bool focus)
        {
            Cursor.lockState = CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !CursorLocked;
        }
    }
}
