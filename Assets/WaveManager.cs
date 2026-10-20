using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;           // Ej: "Oleada 1: Cacería de Aluxes"
        public GameObject[] enemyPrefabs; // Prefabs asignados desde Assets
        public int enemyCount;            // Cantidad de enemigos
        public float spawnInterval;       // Tiempo entre apariciones
    }

    public Wave[] waves;                  // Lista de oleadas en el Inspector
    public Transform[] spawnPoints;       // Puntos de generación en el cenote
    public TextMeshProUGUI waveTextUI;    // Texto en pantalla para la oleada
    public ExperienceSystem expSystem;    // Referencia al sistema de XP

    private int currentWaveIndex = 0;
    private int enemiesRemaining;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            if (waveTextUI != null)
            {
                waveTextUI.text = "¡VICTORIA EN EL CENOTE!";
            }
            yield break;
        }

        Wave currentWave = waves[currentWaveIndex];

        if (waveTextUI != null)
        {
            waveTextUI.text = currentWave.waveName;
        }
        
        yield return new WaitForSeconds(2f);

        if (waveTextUI != null)
        {
            waveTextUI.text = ""; // Borra el texto de la pantalla
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("⚠️ No has asignado Spawn Points en el GeneradorOleadas.");
            yield break;
        }

        if (currentWave.enemyPrefabs == null || currentWave.enemyPrefabs.Length == 0)
        {
            Debug.LogError("⚠️ No has asignado ningún Prefab de enemigo en la oleada actual.");
            yield break;
        }

        enemiesRemaining = currentWave.enemyCount;
        isSpawning = true;

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyPrefab = currentWave.enemyPrefabs[Random.Range(0, currentWave.enemyPrefabs.Length)];

            if (enemyPrefab != null && spawnPoint != null)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.onEnemyKilled += OnEnemyDefeated;
                }
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        isSpawning = false;
        ComprobarFinDeOleada();
    }

    void OnEnemyDefeated(int expReward)
    {
        enemiesRemaining--;

        if (expSystem != null)
        {
            expSystem.AddExperience(expReward);
        }

        ComprobarFinDeOleada();
    }

    void ComprobarFinDeOleada()
    {
        if (enemiesRemaining <= 0 && !isSpawning)
        {
            currentWaveIndex++;
            if (expSystem != null)
            {
                expSystem.TriggerSkillSelection();
            }
            StartCoroutine(StartNextWave());
        }
    }
}