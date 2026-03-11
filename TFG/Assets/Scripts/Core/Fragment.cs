using System.Collections.Generic;

[System.Serializable]
// Representa un fragment executable i la seva cua de comandes.
public class Fragment
{
    public string identifier;
    public int line;
    public List<string> commandList = new List<string>();
    private int currentCommandIndex = 0;

    // Crea el fragment amb l'ID logic i la linia de codi associada.
    public Fragment(string id, int lineNum) { identifier = id; line = lineNum; }

    // Afegeix una comanda a la sequencia d'execucio del fragment.
    public void AddCommand(string cmd) => commandList.Add(cmd);

    // Reinicia el recorregut de comandes.
    public void BeginIterate() => currentCommandIndex = 0;

    // Indica si ja s'han executat totes les comandes del fragment.
    public bool IsFinished() => currentCommandIndex >= commandList.Count;

    // Retorna la seguent comanda i avanca el cursor intern.
    public string NextCommand() => !IsFinished() ? commandList[currentCommandIndex++] : null;
}