using Unity.VisualScripting;
using UnityEngine;

namespace FPController
{
    public class TestDamage : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                FirstPersonController.OnTakeDamage(15);
            }
        }
    }
}
