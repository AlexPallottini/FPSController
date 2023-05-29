using UnityEngine;
using UnityEngine.UI;

namespace FPController
{
    public class UI : MonoBehaviour
    {
        [SerializeField] private Text healthText = default;
        [SerializeField] private Text staminaText = default;

        private void Start()
        {
            UpdateHealth(100);
            UpdateStamina(100);
        }

        private void OnEnable()
        {
            FirstPersonController.OnDamage += UpdateHealth;
            FirstPersonController.OnHeal += UpdateHealth;
            FirstPersonController.OnStaminaChange += UpdateStamina;
        }
        private void OnDisable()
        {
            FirstPersonController.OnDamage -= UpdateHealth;
            FirstPersonController.OnHeal -= UpdateHealth;
            FirstPersonController.OnStaminaChange -= UpdateStamina;
        }

        private void UpdateHealth(float currentHealth)
        {
            healthText.text = currentHealth.ToString("00");
        }

        private void UpdateStamina(float currentStamina)
        {
            staminaText.text = currentStamina.ToString("00");
        }
    }
}
