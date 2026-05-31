using UnityEngine;
using System.Collections.Generic;
using System;

public class LevelParser : MonoBehaviour
{
    private enum ParseState { ReadCode, ReadLogic }
    private ParseState currentState = ParseState.ReadCode;
    private List<string> sourceCodeLines = new List<string>();
    private string currentFragmentID = "";

    public List<string> GetSourceCodeLines() => sourceCodeLines;

    // Parseja el fitxer i inicialitza l'execucio en acabar.
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

    // Extreu els IDs <n> del codi i conserva el text visible net.
    private void ProcessCodeLine(string line, int lineIndex, GameManager manager)
    {
        var textLine = new System.Text.StringBuilder();
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
                if (!manager.HasFragment(readId))
                    manager.RegisterFragment(readId, new Fragment(readId, lineIndex));
                readId = ""; continue;
            }

            if (readingTag) readId += line[i];
            else textLine.Append(line[i]);
        }
        sourceCodeLines.Add(textLine.ToString());
    }

    // Associa cada comanda de LOGIC amb el fragment actual.
    private void ProcessLogicLine(string line, GameManager manager)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        if (line.StartsWith("<") && line.EndsWith(">"))
            currentFragmentID = line.Substring(1, line.Length - 2);
        else if (manager.HasFragment(currentFragmentID))
            manager.AddCommandToFragment(currentFragmentID, line);
    }
}
