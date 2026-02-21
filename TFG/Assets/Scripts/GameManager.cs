using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    LevelParser parser;
    public List<string> sourceCodeLines; // El text que es veu al panell [cite: 133]
    public TextAsset levelFile; // El fitxer de text que conté el codi font i les etiquetes [cite: 132]
    public Dictionary<string, Fragment> fragments = new Dictionary<string, Fragment>();

    public Transform codePanel; 
    public GameObject textPrefab;

    void Start() {
        parser = GetComponent<LevelParser>();
        LoadLevel();
        UpdateCodeUI();
    }

    public void OnPlayerAction(string actionType, object data) {
        //[cite_start]// Valida si l'acció del jugador coincideix amb la Task actual [cite: 154, 155]
       // [cite_start]// Si és correcte, avança el Program Counter [cite: 137]
    }
    private void LoadLevel() {
        // Llegeix el fitxer de text i omple sourceCodeLines i fragments [cite: 132]
        parser.ParseLevel(levelFile, this);
        
    }
    void UpdateCodeUI() {
        foreach (string line in sourceCodeLines) {
            GameObject newLine = Instantiate(textPrefab, codePanel);
            newLine.GetComponent<TMP_Text>().text = line; 
        }
    }
}