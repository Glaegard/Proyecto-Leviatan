// File: Ship.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Ship : MonoBehaviour
{
    [Header("Estadísticas")]
    public int attack;
    public int maxHealth;
    public int currentHealth;
    public int crewCount;

    [Header("Propietario y carril")]
    public bool isPlayer;
    public int laneIndex;

    [Header("Movimiento")]
    public float moveInterval = 3f;
    public float moveDistance = 5f;
    private Vector3 targetPosition;
    private float smoothSpeed;

    [Header("UI Interna")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text statsText;
    [SerializeField] private Text crewText;

    public GameObject uiElementsPrefab;

    private bool isSinking = false;
    [HideInInspector] public bool isFighting = false;

    private Rigidbody rb;
    private Collider coll;

    /// <summary>
    /// Inicialización completa al spawnear un barco en el mundo 3D.
    /// </summary>
    public void Initialize(
        bool playerTeam,
        int baseAttack,
        int baseHealth,
        Vector3 targetPos,
        float moveInterval,
        float moveDistance,
        int lane)
    {

        isPlayer = playerTeam;
        attack = baseAttack;
        maxHealth = currentHealth = baseHealth;
        crewCount = 1;
        laneIndex = lane;

        targetPosition = targetPos;
        this.moveInterval = moveInterval;
        this.moveDistance = moveDistance;
        smoothSpeed = moveDistance / moveInterval;

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        rb.isKinematic = true;
        coll.isTrigger = true;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        UpdateUI();

        StartCoroutine(MoveRoutine());
    }

    /// <summary>
    /// Genera una vista previa inmóvil del barco en UI.
    /// </summary>
    public void InitializePreview(bool playerTeam, int attack, int health, int lane)
    {
        isPlayer = playerTeam;
        this.attack = attack;
        maxHealth = currentHealth = health;
        crewCount = 1;
        laneIndex = lane;

        if (!isPlayer)
        {
            uiElementsPrefab.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
        // No participará en física ni movimiento
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        if (rb != null) rb.isKinematic = true;
        if (coll != null) coll.enabled = false;

        UpdateUI();
    }

    /// <summary>
    /// Actualiza estadísticas de la vista previa al agregar cartas.
    /// </summary>
    public void SetStats(int attack, int health, int crew)
    {
        this.attack = attack;
        maxHealth = health;
        currentHealth = health;
        crewCount = crew;

        if (!isPlayer)
        {
            uiElementsPrefab.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
        UpdateUI();
    }

    private IEnumerator MoveRoutine()
    {
        while (!isFighting && !isSinking)
        {
            float step = smoothSpeed * Time.deltaTime;
            transform.position += isPlayer ? Vector3.forward * step
                                           : Vector3.back * step;

            // Al llegar al final → abordaje
            if ((isPlayer && transform.position.z >= targetPosition.z) ||
                (!isPlayer && transform.position.z <= targetPosition.z))
            {
                GameManager.Instance.TriggerBoarding(isPlayer);
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

        // Solo uno inicia el combate
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

        bool iSurvives = currentHealth > 0;
        bool enemySurvives = enemy.currentHealth > 0;

        if (!iSurvives) Sink();
        if (!enemySurvives) enemy.Sink();

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

    /// <summary>
    /// Actualiza los elementos UI del barco (barra, textos).
    /// </summary>
    public void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = Mathf.Max(0, currentHealth);

        if (statsText != null)
            statsText.text = $"{attack}/{Mathf.Max(0, currentHealth)}";

        if (crewText != null)
            crewText.text = crewCount.ToString();
    }

    /// <summary>
    /// Activa/desactiva un indicador visual de resaltado.
    /// </summary>
    public void SetHighlight(bool active)
    {
        // Implementar según tu indicador
    }
}
