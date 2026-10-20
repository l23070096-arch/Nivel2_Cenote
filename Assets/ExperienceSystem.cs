using UnityEngine;
using TMPro;

public class ExperienceSystem : MonoBehaviour
{
    public int currentExp = 0;
    public int expToNextLevel = 100;
    public int currentLevel = 1;

    public GameObject skillSelectionPanel; // UI Panel con 3 botones de habilidades[cite: 6, 12, 13]
    public TextMeshProUGUI levelTextUI;

    public void AddExperience(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f);
            UpdateUI();
        }
    }

    public void TriggerSkillSelection()
    {
        // Pausa el juego y muestra el panel con 3 opciones de mejora[cite: 6, 12, 13, 24]
        Time.timeScale = 0f; 
        if (skillSelectionPanel != null)
            skillSelectionPanel.SetActive(true);
    }

    // Métodos para asignar a los 3 Botones en Unity:
    public void SelectSkill1_DamageBoost()
    {
        Debug.Log("Habilidad Elegida: +Aumento de Daño (Macuahuitl/Arco)");
        ResumeGame();
    }

    public void SelectSkill2_HealthRegen()
    {
        Debug.Log("Habilidad Elegida: +Regeneración de Vida");
        ResumeGame();
    }

    public void SelectSkill3_SpeedBoost()
    {
        Debug.Log("Habilidad Elegida: +Velocidad de Movimiento");
        ResumeGame();
    }

    private void ResumeGame()
    {
        if (skillSelectionPanel != null)
            skillSelectionPanel.SetActive(false);
        Time.timeScale = 1f; // Reanuda el juego
    }

    void UpdateUI()
    {
        if (levelTextUI != null)
            levelTextUI.text = "Nivel " + currentLevel;
    }
}