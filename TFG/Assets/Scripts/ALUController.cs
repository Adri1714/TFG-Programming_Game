using UnityEngine;

public class ALUController : MonoBehaviour
{
    public string leftOperand = "0";
    public string rightOperand = "0";
    public string currentOperator = "+";
    public string result = "0";

    public void UpdateCalculation() {
        int left = int.Parse(leftOperand);
        int right = int.Parse(rightOperand);

        switch (currentOperator) {
            case "+": result = (left + right).ToString(); break;
            case "-": result = (left - right).ToString(); break;
            case "*": result = (left * right).ToString(); break;
            case "/": result = right != 0 ? (left / right).ToString() : "ERR"; break; 
        }
        //
    }
}