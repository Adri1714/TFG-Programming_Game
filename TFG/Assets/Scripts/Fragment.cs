using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Fragment
{
    public string identifier;        // El número ID de l'etiqueta < >
    public int line;                 // Línia del codi font original
    public int start;                // Caràcter inicial per al ressaltat
    public int end;                  // Caràcter final per al ressaltat
    
    public List<string> commandList = new List<string>(); // Llista de Commands/Tasks
    public int currentCommandIndex = 0;
    public bool initialized = false;

    public Fragment(string id, int lineNum, int startChar)
    {
        this.identifier = id;
        this.line = lineNum;
        this.start = startChar;
        this.initialized = true; //
    }

    public void AddCommand(string command)
    {
        commandList.Add(command); //
    }

    public void BeginIterate()
    {
        currentCommandIndex = 0; // Reinicia el punter per a bucles
    }

    public string NextCommand()
    {
        if (!IsFinished())
        {
            string cmd = commandList[currentCommandIndex];
            currentCommandIndex++;
            return cmd; // Retorna la següent acció a fer
        }
        return null;
    }

    public bool IsFinished()
    {
        return currentCommandIndex >= commandList.Count; //
    }
}