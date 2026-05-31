using UnityEngine;

// Activa/desactiva les trampes de l'escena segons el nivell seleccionat.
public class LevelHazardConfig : MonoBehaviour
{
    [System.Serializable]
    public class LevelHazards
    {
        public int levelNumber = 1;
        public GameObject[] hazardsToEnable;
    }

    [Tooltip("Totes les trampes de l'escena. Es desactiven al començar.")]
    [SerializeField] private GameObject[] allHazards;

    [Tooltip("Quines trampes s'activen per a cada nivell.")]
    [SerializeField] private LevelHazards[] perLevel;

    private void Awake()
    {
        if (allHazards != null)
            foreach (GameObject hazard in allHazards)
                if (hazard != null) hazard.SetActive(false);

        int level = GameSession.SelectedLevelNumber;
        if (perLevel == null) return;

        foreach (LevelHazards config in perLevel)
        {
            if (config.levelNumber != level || config.hazardsToEnable == null) continue;
            foreach (GameObject hazard in config.hazardsToEnable)
                if (hazard != null) hazard.SetActive(true);
        }
    }
}
