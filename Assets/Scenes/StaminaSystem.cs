
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;

    public float kickStaminaCost = 25f;
    public float staminaRegenRate = 15f;
    public float regenDelay = 2f;

    [Header("UI")]
    public Image staminaBar;

    private bool canRegenerate = true;

    void Start()
    {
        currentStamina = maxStamina;
        UpdateStaminaUI();
    }

    void Update()
    {
        if (canRegenerate && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;

            currentStamina = Mathf.Clamp(
                currentStamina, 0f, maxStamina
            );

            UpdateStaminaUI();
        }
    }

    public bool UseStamina()
    {
        if (currentStamina < kickStaminaCost)
        {
            return false;
        }

        currentStamina -= kickStaminaCost;
        UpdateStaminaUI();

        StopAllCoroutines();
        StartCoroutine(StaminaRegenDelay());

        return true;
    }

    IEnumerator StaminaRegenDelay()
    {
        canRegenerate = false;

        yield return new WaitForSeconds(regenDelay);

        canRegenerate = true;
    }

    void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = currentStamina / maxStamina;
        }
    }
}