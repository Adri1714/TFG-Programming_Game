using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 input;

    void Update() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (input != Vector2.zero) {
            // Convertim l'input de teclat a coordenades isomètriques (X, Y -> ISO)
            Vector3 moveDir = new Vector3(input.x - input.y, 0, (input.x + input.y) * 0.5f);
            transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        }
    }
}