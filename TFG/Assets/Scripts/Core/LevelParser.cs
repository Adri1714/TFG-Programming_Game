using UnityEngine;
using System.Collections.Generic;
using System;

// Llegeix el fitxer de nivell i separa codi font de logica executable.
public class LevelParser : MonoBehaviour
{
    private enum ParseState { ReadCode, ReadLogic }
    private ParseState currentState = ParseState.ReadCode;
    private List<string> sourceCodeLines = new List<string>();
    private string currentFragmentID = "";

    // Exposa les linies per a la UI del codi.
    public List<string> GetSourceCodeLines() => sourceCodeLines;


    // Parseja el fitxer complet i inicialitza l'execucio en acabar.
    public void ParseLevel(TextAsset levelFile, GameManager manager)
    {
        if (levelFile == null) return;

        sourceCodeLines.Clear();
        string[] lines = levelFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed == "LOGIC") { currentState = ParseState.ReadLogic; continue; }

            if (currentState == ParseState.ReadCode) ProcessCodeLine(line, sourceCodeLines.Count, manager);
            else ProcessLogicLine(trimmed, manager);
        }
        
        manager.InitializeExecution();
    }

    // Extreu IDs <n> de la part de codi i conserva el text visible net.
    private void ProcessCodeLine(string line, int lineIndex, GameManager manager)
    {
        string textLine = "";
        string readId = "";
        bool readingTag = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (!readingTag && line[i] == '<' && (i+1 < line.Length && char.IsDigit(line[i+1])))
            {
                readingTag = true; continue;
            }
            if (readingTag && line[i] == '>')
            {
                readingTag = false;
                if (!manager.fragments.ContainsKey(readId))
                    manager.fragments.Add(readId, new Fragment(readId, lineIndex));
                readId = ""; continue;
            }

            if (readingTag) readId += line[i];
            else textLine += line[i];
        }
        sourceCodeLines.Add(textLine);
    }

    // Associa cada comanda de LOGIC amb el fragment actual.
    private void ProcessLogicLine(string line, GameManager manager)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        if (line.StartsWith("<") && line.EndsWith(">"))
            currentFragmentID = line.Substring(1, line.Length - 2);
        else if (manager.fragments.ContainsKey(currentFragmentID))
            manager.fragments[currentFragmentID].AddCommand(line);
    }
}