using UnityEngine;

namespace FPController
{
    public class TestInteractable : Interactable
    {
        public override void OnFocus()
        {
            Debug.Log($"Looking at {this.gameObject.name}");
        }

        public override void OnInteract()
        {
            Debug.Log($"Interacted with {this.gameObject.name}");
        }

        public override void OnLoseFocus()
        {
            Debug.Log($"Stopped looking at {this.gameObject.name}");
        }
    }
}
