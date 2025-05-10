// ====================================================================
// Ship.cs – lógica de barcos: movimiento, combate y hundimiento
// ====================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Ship : MonoBehaviour
{
    //——————— Atributos ———————//
    public int attack;
    public int maxHealth;
    public int currentHealth;
    public int crewCount;

    public bool isPlayer;
    public int laneIndex;

    private Vector3 targetPosition;
    private float smoothSpeed;

    // UI
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text statsText;
    [SerializeField] private Text crewText;
    [SerializeField] private GameObject highlightIndicator;

    // Estado
    private bool isSinking = false;
    [HideInInspector] public bool isFighting = false;

    private Rigidbody rb;
    private Collider coll;

    //——————— Inicialización ———————//
    public void Initialize(
        bool playerTeam, int baseAttack, int baseHealth,
        Vector3 targetPos, float moveInterval, float moveDistance, int laneIndex)
    {
        isPlayer = playerTeam;
        attack = baseAttack;
        maxHealth = currentHealth = baseHealth;
        crewCount = 1;
        this.laneIndex = laneIndex;

        targetPosition = targetPos;
        smoothSpeed = moveDistance / moveInterval;

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        rb.isKinematic = true;
        coll.isTrigger = true;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        UpdateUI();

        StartCoroutine(MoveRoutine());
    }

    //——————— Rutinas ———————//
    private IEnumerator MoveRoutine()
    {
        while (!isFighting && !isSinking)
        {
            float step = smoothSpeed * Time.deltaTime;
            transform.position += isPlayer ? Vector3.forward * step
                                           : Vector3.back * step;

            // Llegada al final del carril → abordaje
            if ((isPlayer && transform.position.z >= targetPosition.z) ||
                (!isPlayer && transform.position.z <= targetPosition.z))
            {
                if (GameManager.Instance != null)
                {
                    if (isPlayer) GameManager.Instance.playerController.boardingCount++;
                    else GameManager.Instance.aiController.boardingCount++;

                    GameManager.Instance.TriggerBoarding();
                }
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("AtackCollider")) return;

        Ship otherShip = other.GetComponentInParent<Ship>();
        if (otherShip == null || otherShip.isPlayer == isPlayer) return;
        if (isFighting || otherShip.isFighting) return;

        // asegura que solo uno inicia la corrutina de combate
        if (GetInstanceID() < otherShip.GetInstanceID())
        {
            isFighting = true;
            otherShip.isFighting = true;
            StartCoroutine(CombatRoutine(otherShip));
        }
    }

    private IEnumerator CombatRoutine(Ship enemy)
    {
        while (currentHealth > 0 && enemy.currentHealth > 0)
        {
            currentHealth -= enemy.attack;
            enemy.currentHealth -= attack;

            UpdateUI();
            enemy.UpdateUI();

            yield return new WaitForSeconds(1f);
        }

        // determinar supervivientes
        bool iSurvives = currentHealth > 0;
        bool enemySurvives = enemy.currentHealth > 0;

        if (!iSurvives) Sink();
        if (!enemySurvives) enemy.Sink();

        // reiniciar movimiento de los que siguen vivos
        if (iSurvives)
        {
            isFighting = false;
            StartCoroutine(MoveRoutine());
        }
        if (enemySurvives)
        {
            enemy.isFighting = false;
            enemy.StartCoroutine(enemy.MoveRoutine());
        }
    }

    private void Sink()
    {
        if (isSinking) return;
        isSinking = true;
        coll.enabled = false;
        StartCoroutine(SinkRoutine());
    }

    private IEnumerator SinkRoutine()
    {
        float elapsed = 0f, duration = 1f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            transform.position = start + Vector3.down * (elapsed / duration * 2f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    //——————— Utilidades ———————//
    public void UpdateUI()
    {
        if (healthBar != null) healthBar.value = Mathf.Max(currentHealth, 0);
        if (statsText != null) statsText.text = $"{attack}/{Mathf.Max(currentHealth, 0)}";
        if (crewText != null) crewText.text = crewCount.ToString();
    }

    public void SetHighlight(bool active)
    {
        // Activa o desactiva el indicador visual de resaltado del barco
        if (highlightIndicator != null)
            highlightIndicator.SetActive(active);
    }

    public void InitializePreview(bool playerTeam, int attack, int health, int lane)
    {
        isPlayer = playerTeam;
        this.attack = attack;
        this.maxHealth = this.currentHealth = health;
        crewCount = 1;
        laneIndex = lane;

        UpdateUI();
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        if (rb != null) rb.isKinematic = true;
        if (coll != null) coll.enabled = false;
    }

    public void SetStats(int attack, int health, int crew)
    {
        this.attack = attack;
        this.maxHealth = health;
        this.currentHealth = health;
        this.crewCount = crew;
        UpdateUI();
    }

}
