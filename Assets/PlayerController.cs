using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Estadísticas")]
    public float moveSpeed = 5f;
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    [Header("Efecto de Daño")]
    [Tooltip("¿Cuánto tiempo dura el color rojo en segundos?")]
    public float damageFlashDuration = 0.15f; 
    [Tooltip("¿Cuántas veces parpadea rojo/normal?")]
    public int flashCount = 3; 

    [Header("Sistema de Ataque")]
    public GameObject projectilePrefab;     // Prefab del proyectil/lanza
    public bool dispararAutomatico = false; // DESMARCA para clic manual, MARCA para automático
    public float attackInterval = 0.3f;      // Cadencia/tiempo entre disparos
    public float attackRange = 8f;           // Rango de búsqueda (para disparo automático)

    [Header("Límites del Mapa")]
    public bool usarLimitesPorCodigo = true;
    public float minX = -5.099996f;
    public float maxX = 4.299996f;
    public float minY = -2.809998f;
    public float maxY = 1.089999f;

    [Header("Animación Procedural (Rebote)")]
    public bool usarAnimacionProcedural = true;
    public float bounceSpeed = 14f;      // Frecuencia/velocidad del paso
    public float bounceAmount = 0.04f;    // Cuánto se estira y encoge el personaje
    public float tiltAngle = 6f;         // Ángulo de inclinación lateral
    public float baseScale = 0.3f;       // Escala original de Balam

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; 
    private Vector2 moveInput;
    private bool isFlashing = false; 
    private float attackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 

        currentHealth = maxHealth;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("⚠️ El Player no tiene un componente SpriteRenderer en el mismo objeto que el PlayerController.");
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // Giro visual del personaje respetando la escala base
        if (moveInput.x > 0)
            transform.localScale = new Vector3(baseScale, transform.localScale.y, baseScale);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(-baseScale, transform.localScale.y, baseScale);

        // Control de temporizador para disparos
        attackTimer += Time.deltaTime;

        if (dispararAutomatico)
        {
            // MODO AUTOMÁTICO
            if (attackTimer >= attackInterval)
            {
                AtacarEnemigoMasCercano();
                attackTimer = 0f;
            }
        }
        else
        {
            // MODO MANUAL (Clic Izquierdo del Mouse)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && attackTimer >= attackInterval)
            {
                DispararHaciaCursor();
                attackTimer = 0f;
            }
        }

        // Animación de rebote al caminar
        if (usarAnimacionProcedural)
        {
            AplicarAnimacionProcedural();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        // Bloquea a Balam dentro de las coordenadas del mapa
        if (usarLimitesPorCodigo)
        {
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }

    // Animación de rebote (Squash & Stretch) e inclinación
    void AplicarAnimacionProcedural()
    {
        bool estaMoviendose = moveInput.sqrMagnitude > 0.01f;

        if (estaMoviendose)
        {
            // 1. Inclinación lateral
            float zRotation = Mathf.Sin(Time.time * bounceSpeed) * tiltAngle;
            transform.rotation = Quaternion.Euler(0, 0, zRotation);

            // 2. Estiramiento vertical (Squash & Stretch)
            float factorY = Mathf.Sin(Time.time * bounceSpeed * 2f) * bounceAmount;
            
            // Mantiene el sentido de dirección (X positivo o negativo)
            float signoX = transform.localScale.x >= 0 ? 1f : -1f;

            transform.localScale = new Vector3(
                signoX * baseScale,
                baseScale + factorY,
                baseScale
            );
        }
        else
        {
            // Retorno suave a la posición estándar al detenerse
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 12f);

            float signoX = transform.localScale.x >= 0 ? 1f : -1f;
            Vector3 escalaObjetivo = new Vector3(signoX * baseScale, baseScale, baseScale);
            transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, Time.deltaTime * 12f);
        }
    }

    // Disparo manual hacia el cursor
    void DispararHaciaCursor()
    {
        if (projectilePrefab == null || Camera.main == null) return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 direccion = (mouseWorldPos - transform.position).normalized;

        GameObject proyectilObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Proyectil proyectilScript = proyectilObj.GetComponent<Proyectil>();

        if (proyectilScript != null)
        {
            proyectilScript.Inicializar(direccion);
        }
    }

    // Disparo automático al enemigo más cercano
    void AtacarEnemigoMasCercano()
    {
        if (projectilePrefab == null) return;

        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject enemigoMasCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (GameObject enemigo in enemigos)
        {
            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia < distanciaMinima && distancia <= attackRange)
            {
                distanciaMinima = distancia;
                enemigoMasCercano = enemigo;
            }
        }

        if (enemigoMasCercano != null)
        {
            Vector2 direccionDisparo = (enemigoMasCercano.transform.position - transform.position).normalized;
            
            GameObject proyectilObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Proyectil proyectilScript = proyectilObj.GetComponent<Proyectil>();

            if (proyectilScript != null)
            {
                proyectilScript.Inicializar(direccionDisparo);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        if (currentHealth < 0) currentHealth = 0;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Vida actual de Balam: " + currentHealth);

        if (!isFlashing && currentHealth > 0) 
        {
            StartCoroutine(DamageFlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlashEffect()
    {
        isFlashing = true; 
        Color originalColor = spriteRenderer.color; 

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(damageFlashDuration);
            
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(damageFlashDuration);
        }

        isFlashing = false; 
    }

    void Die()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white; 
        
        Debug.Log("¡Balam ha sido derrotado!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}