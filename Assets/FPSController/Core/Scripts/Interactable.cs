using UnityEngine;

namespace FPController
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract void OnInteract();
        public abstract void OnFocus();
        public abstract void OnLoseFocus();
    }
}
