using System.Collections.Generic;

[System.Serializable]
public class Fragment
{
    public string identifier;
    public int line;
    public List<string> commandList = new List<string>();
    private int currentCommandIndex = 0;

    public Fragment(string id, int lineNum) { identifier = id; line = lineNum; }

    public void AddCommand(string cmd) => commandList.Add(cmd);

    public void BeginIterate() => currentCommandIndex = 0;

    public bool IsFinished() => currentCommandIndex >= commandList.Count;

    // Retorna la seguent comanda i avanca el cursor intern.
    public string NextCommand() => !IsFinished() ? commandList[currentCommandIndex++] : null;
}
