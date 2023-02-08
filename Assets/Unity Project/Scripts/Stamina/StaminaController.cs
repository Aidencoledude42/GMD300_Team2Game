using System.Collections;
using UnityEngine;

/// <summary>
/// Maintains a 'Stamina' resource as a percentage.
/// </summary>
public class StaminaController : MonoBehaviour
{
    public const float MAX_STAMINA_VALUE = 100f;
    private float m_StaminaValue = MAX_STAMINA_VALUE;

    public float CurrentStamina { get => m_StaminaValue; }

    // Regeneration
    private bool m_IsRegenerating = false;

    public bool IsRegenerating { get => m_IsRegenerating; }

    [SerializeField] private float StaminaRegenRate = 1f;

    // Regeneration DeBounce
    [SerializeField] private float DebounceTime = 0.65f;
    private bool m_IsDebouncing = false;
    private IEnumerator m_DebounceCRT;

    private void Start()
    {
        //
    }

    private void Update()
    {
        // Regenerate if possible
        if (!m_IsRegenerating) return;
        RegenerateStamina();
    }

    // + + + + | Functions | + + + +

    /// <summary>
    /// A Coroutine that waits for a given 'time' before allowing regeneration.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator DebounceTimer(float time)
    {
        float deltaTimeHelper = 0f;
        m_IsDebouncing = true;

        while (deltaTimeHelper <= time)
        {
            deltaTimeHelper += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        m_IsDebouncing = false;
    }

    /// <summary>
    /// Handles resetting the debounce coroutine.
    /// </summary>
    private void StartDebounceTimer()
    {
        // If Debounce CRT is running, 
        if (m_IsDebouncing == true)
        {
            //StopAllCoroutines();
            StopCoroutine(m_DebounceCRT);
        }

        // Start Debounce CRT
        m_DebounceCRT = DebounceTimer(DebounceTime);
        StartCoroutine(m_DebounceCRT);
    }

    /// <summary>
    /// Returns whether or not an amount of Stamina can be removed.
    /// </summary>
    /// <param name="amountToRemove"></param>
    /// <returns></returns>
    public bool CanRemoveStaminaAmount(float amountToRemove)
    {
        return CurrentStamina >= amountToRemove;
    }

    /// <summary>
    /// Handles the request to remove some Stamina.
    /// </summary>
    /// <param name="amountToRemove"></param>
    public void RemoveStaminaAmount(float amountToRemove)
    {
        // TODO: Check for race conditions here, shouldn't occur if just one thread but you never know.
        if (!CanRemoveStaminaAmount(amountToRemove)) return;

        m_StaminaValue -= amountToRemove;

        // Start debounce timer
        StartDebounceTimer();
    }

    /// <summary>
    /// Regenerates Stamina and stops regeneration when at full health.
    /// </summary>
    public void RegenerateStamina()
    {
        // No need to regenerate if at max health
        if (m_StaminaValue == MAX_STAMINA_VALUE) return;
        else if (m_StaminaValue + StaminaRegenRate >= MAX_STAMINA_VALUE)
        {
            m_StaminaValue = Mathf.Clamp(m_StaminaValue + StaminaRegenRate, 0f, MAX_STAMINA_VALUE);
            m_IsRegenerating = false;
        }
        else
        {
            m_StaminaValue += StaminaRegenRate;
            m_IsRegenerating = true;
        }
    }
}