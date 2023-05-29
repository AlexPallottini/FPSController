using System.Collections;
using UnityEngine;

namespace FPController
{
    public class Door : Interactable
    {
        [SerializeField] private FirstPersonController playerController;
        [SerializeField]private Animator anim = default;
        private bool isOpen = false;
        private bool canBeInteractedWith = true;

        public override void OnFocus() { }

        public override void OnInteract()
        {
            if(canBeInteractedWith)
            {
                isOpen = !isOpen;

                Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
                Vector3 playerTransformDirection = playerController.transform.position - this.transform.position;
                float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

                anim.SetFloat("dot", dot);
                anim.SetBool("isOpen", isOpen);

                StartCoroutine(AutoClose());
            }
        }

        public override void OnLoseFocus() { }

        private IEnumerator AutoClose()
        {
            while (isOpen)
            {
                yield return new WaitForSeconds(3f);

                if(Vector3.Distance(transform.position, playerController.transform.position) > 3)
                {
                    isOpen = false;

                    anim.SetFloat("dot", 0);
                    anim.SetBool("isOpen", isOpen);
                }
            }
        }

        private void Animator_LockInteraction()
        {
            canBeInteractedWith = false;
        }
        private void Animator_UnLockInteraction()
        {
            canBeInteractedWith = true;
        }
    }
}
