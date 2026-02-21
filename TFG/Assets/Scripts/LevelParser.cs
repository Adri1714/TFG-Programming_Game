using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LevelParser : MonoBehaviour
{
    private enum ParseState { ReadCode, ReadLogic }
    private ParseState currentState = ParseState.ReadCode;

    // Estats temporals per al processament de tags <ID>
    private HashSet<string> activeIds = new HashSet<string>();
    private string currentFragmentID = "";

    /// <summary>
    /// Llegeix el fitxer de nivell i omple les dades al GameManager.
    /// </summary>
    public void ParseLevel(TextAsset levelFile, GameManager manager)
    {
        if (levelFile == null) {
            Debug.LogError("No s'ha proporcionat cap fitxer de nivell (TextAsset).");
            return;
        }

        // Reiniciem estats per si carreguem un nou nivell
        currentState = ParseState.ReadCode;
        activeIds.Clear();
        currentFragmentID = "";
        int lineCount = 0;

        // Dividim el fitxer per línies
        string[] lines = levelFile.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Detectem el canvi de secció a LOGIC
            if (trimmedLine == "LOGIC")
            {
                currentState = ParseState.ReadLogic;
                continue;
            }

            if (currentState == ParseState.ReadCode)
            {
                ProcessCodeLine(line, lineCount, manager);
            }
            else
            {
                ProcessLogicLine(trimmedLine, manager);
            }
            lineCount++;
        }

        // Validació final: tots els fragments s'han d'haver tancat
        if (activeIds.Count > 0) {
            Debug.LogWarning("Hi ha fragments sense tancar (<ID>) al final del fitxer.");
        }
        
        Debug.Log("Parseig completat: " + manager.fragments.Count + " fragments carregats.");
    }

    private void ProcessCodeLine(string line, int lineIndex, GameManager manager)
    {
        string textLine = ""; // Text net que veurà el jugador a la UI
        string readId = "";
        bool readingTag = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            char next = (i + 1 < line.Length) ? line[i + 1] : '\0';

            if (!readingTag && c == '<' && next != ' ' && next != '=')
            {
                readingTag = true;
                readId = "";
                continue;
            }

            if (readingTag)
            {
                if (c == '>')
                {
                    readingTag = false;
                    HandleTag(readId, textLine.Length, lineIndex, manager);
                }
                else
                {
                    readId += c;
                }
            }
            else
            {
                textLine += c;
            }
        }
        manager.sourceCodeLines.Add(textLine); //
    }

    private void HandleTag(string id, int currentCharPos, int lineIndex, GameManager manager)
    {
        // Si l'ID ja és actiu, estem tancant el fragment
        if (activeIds.Contains(id))
        {
            activeIds.Remove(id);
            if (manager.fragments.ContainsKey(id)) {
                manager.fragments[id].end = currentCharPos; // Guardem on acaba el ressaltat
            }
        }
        else // Si és nou, obrim un fragment
        {
            activeIds.Add(id);
            Fragment newFrag = new Fragment(id, lineIndex, currentCharPos);
            manager.fragments.Add(id, newFrag); //
        }
    }

    private void ProcessLogicLine(string line, GameManager manager)
    {
        if (string.IsNullOrWhiteSpace(line)) return;

        // Si és un header d'ID (ex: <1>)
        if (line.StartsWith("<") && line.EndsWith(">") && !line.StartsWith("< "))
        {
            currentFragmentID = line.Substring(1, line.Length - 2);
            if (!manager.fragments.ContainsKey(currentFragmentID)) {
                Debug.LogError("Error en LOGIC: El fragment <" + currentFragmentID + "> no es va definir al CODI.");
            }
        }
        else if (!string.IsNullOrEmpty(currentFragmentID))
        {
            // Afegim la comanda o tasca al fragment actiu
            manager.fragments[currentFragmentID].AddCommand(line); //
        }
    }
}