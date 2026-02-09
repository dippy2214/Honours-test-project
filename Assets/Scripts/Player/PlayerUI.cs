using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : NetworkBehaviour
{
    [Header("References")]
    public Canvas playerCanvas;

    [SerializeField] private Image healthbarFill;
    [SerializeField] private Image damageOverlay;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject RoundEndText;

    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private float fadeSpeed = 1.5f;

    private PlayerHealth playerHealth;
    private float damageTimer;

    private void Awake()
    {
        playerCanvas = GetComponentInChildren<Canvas>(true);

        if (playerCanvas != null)
        {
            healthbarFill = playerCanvas.transform.Find("Healthbar/Fill").GetComponent<Image>();
            damageOverlay = playerCanvas.transform.Find("Damage Overlay").GetComponent<Image>();
            damageOverlay.color = new Color(1f, 0f, 0f, 0);

            promptText = playerCanvas.transform.Find("PromptText").GetComponent<TextMeshProUGUI>();
            promptText.text = "";

            RoundEndText = playerCanvas.transform.Find("Round end text").gameObject;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        playerCanvas.gameObject.SetActive(true);
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Health.OnValueChanged += OnHealthChanged;
            UpdateHealthUI(playerHealth.Health.Value);
        }
    }

    new private void OnDestroy()
    {
        if (playerHealth != null && IsOwner)
            playerHealth.Health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (!IsOwner) return;

        UpdateHealthUI(newValue);

        if (newValue < oldValue)
        {
            damageTimer = 0f;
            damageOverlay.color = new Color(1f, 0f, 0f, 1f);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (damageOverlay.color.a > 0f)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer > damageFlashDuration)
            {
                float alpha = Mathf.Max(damageOverlay.color.a - Time.deltaTime * fadeSpeed, 0f);
                damageOverlay.color = new Color(1f, 0f, 0f, alpha);
            }
        }
    }

    private void UpdateHealthUI(float value)
    {
        if (healthbarFill != null)
            healthbarFill.fillAmount = value / playerHealth.maxHealth;
    }

    public void SetPrompt(string message)
    {
        if (!IsOwner) return;

        if (promptText != null)
            promptText.text = message;
    }

    public void ClearPrompt()
    {
        if (!IsOwner) return;

        if (promptText != null)
            promptText.text = "";
    }

    public void RoundEndTextEnable(string text)
    {
        RoundEndText.GetComponent<TMP_Text>().text = text;
        RoundEndText.SetActive(true);
    }

    public void RoundEndTextDisable()
    {
        RoundEndText.SetActive(false);
    }
}