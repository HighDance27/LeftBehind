using System.Collections;
using TopDown.Audio;
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int currentHealth;
    [SerializeField] private int maximumHealth;

    [SerializeField] private int currentArmor;
    [SerializeField] private int maximumArmor;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;

    public int CurrentArmor => currentArmor;
    public int MaximumArmor => maximumArmor;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] public AudioClip wallSFX;

    private UIManager uIManager;
    private bool hasDied = false;
    private bool triggersGameOver;

    private void Awake()
    {
        triggersGameOver = CompareTag("Player"); // phải có tag để kích hoạt bool gameover
        if (triggersGameOver) uIManager = FindObjectOfType<UIManager>();
    }
    public float RemainingHealthPercentage
    {
        get
        {
            return currentHealth / (float)maximumHealth; //Chuyển kiểu dữ liệu để update UI thanh máu
        }
    }

    public float RemainingArmorPercentage
    {
        get
        {
            return currentArmor / (float)maximumArmor;
        }
    }

    public bool IsInvincible { get; set; }

    public UnityEvent OnDied;
    public UnityEvent OnDamaged;
    public UnityEvent OnHealthChanged;
    public UnityEvent OnArmorChanged;

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth == -1)    //wall
        {
            return;
        }

        if (currentHealth == 0)
        {
            return;
        }

        if (IsInvincible)
        {
            return;
        }

        if (currentArmor > 0)
        {
            if (currentArmor >= damageAmount)
            {
                // Nếu Giáp chịu được toàn bộ sát thương
                currentArmor -= damageAmount;
                damageAmount = 0;
                OnArmorChanged.Invoke();
            }
            else
            {
                // Nếu sát thương lớn hơn Giáp
                damageAmount -= currentArmor; // Tính phần sát thương dư
                currentArmor = 0;
                OnArmorChanged.Invoke();
            }
        }

        // Nếu vẫn còn sát thương dư (hoặc không có giáp), trừ vào máu
        if (damageAmount > 0)
        {
            currentHealth -= damageAmount;
            OnHealthChanged.Invoke();

            if (currentHealth < 0)
            {
                currentHealth = 0;
            }

            if (currentHealth == 0 && !hasDied)
            {
                hasDied = true;
                SoundManager.Instance?.PlaySound(dieSound);
                OnDied.Invoke();
                StartCoroutine(GameOverCoroutine(3.5f));
            }
            else
            {
                SoundManager.Instance?.PlaySound(hurtSound);
                OnDamaged.Invoke();
            }
        }
        else
        {
            // Vẫn phát âm thanh bị đau dù chỉ mất giáp (tùy chọn, có thể bỏ nếu muốn)
            SoundManager.Instance?.PlaySound(hurtSound);
            OnDamaged.Invoke();
        }
    }

    public void ExplodeVFX(ParticleSystem explodeVFX)
    {
        Instantiate(explodeVFX, gameObject.transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void SetHealth(int amount)
    {
        currentHealth = amount;

        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }

        OnHealthChanged.Invoke();
    }

    public void SetArmor(int amount)
    {
        currentArmor = amount;

        if (currentArmor > maximumArmor)
        {
            currentArmor = maximumArmor;
        }

        OnArmorChanged.Invoke();
    }

    public void AddArmor(int amountToAdd)
    {
        if (currentHealth <= 0) return;

        if (currentArmor == maximumArmor) return;

        currentArmor += amountToAdd;

        if (currentArmor > maximumArmor)
        {
            currentArmor = maximumArmor;
        }

        OnArmorChanged.Invoke();
    }

    public void AddHealth(int amountToAdd)
    {
        if (currentHealth == -1)
        {
            return;
        }

        if (currentHealth == maximumHealth)
        {
            return;
        }

        currentHealth += amountToAdd;

        OnHealthChanged.Invoke();

        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
    }

    private IEnumerator GameOverCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        uIManager?.GameOver();
    }
}
