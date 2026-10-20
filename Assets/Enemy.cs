using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Ajustes del Enemigo")]
    public float speed = 2f;          
    public float health = 30f;          // Vida del enemigo
    public float damage = 15f;          
    public int expReward = 20;          

    public Action<int> onEnemyKilled;   

    private Transform playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, 
                playerTransform.position, 
                speed * Time.deltaTime
            );

            if (playerTransform.position.x > transform.position.x)
                transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            else
                transform.localScale = new Vector3(-0.2f, 0.2f, 0.2f);
        }
    }

    // Método para recibir daño desde los proyectiles
    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        onEnemyKilled?.Invoke(expReward);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarContacto(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcesarContacto(collision.gameObject);
    }

    private void ProcesarContacto(GameObject objetoTocado)
    {
        if (objetoTocado.CompareTag("Player"))
        {
            PlayerController player = objetoTocado.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
    }
}