using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Ajustes del Proyectil")]
    public float speed = 12f;
    public float damage = 15f;
    public float lifetime = 3f;

    private Vector2 direccionMovimiento;
    private bool estaInicializado = false;

    public void Inicializar(Vector2 direccion)
    {
        direccionMovimiento = direccion.normalized;
        estaInicializado = true;

        // Orienta el gráfico hacia la dirección a la que vuela
        float angle = Mathf.Atan2(direccionMovimiento.y, direccionMovimiento.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Se destruye automáticamente después de cierto tiempo para liberar memoria
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (estaInicializado)
        {
            // Mueve el proyectil cuadro a cuadro en dirección al objetivo
            transform.position += (Vector3)(direccionMovimiento * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject); // Se destruye al impactar con un Alux
        }
    }
}