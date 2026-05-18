using System.Collections.Generic;
using UnityEngine;

// Gestiona moviment, seleccio d'objectiu i interaccions del jugador.
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public Transform carryPoint;
    public float interactionRadius = 1.5f; 
    public GameObject carriedCube;

    private Rigidbody rb;
    private Vector2 input;
    private readonly Dictionary<string, float> moveSpeedModifiers = new Dictionary<string, float>();
    private readonly HashSet<string> reversedInputSources = new HashSet<string>();

    // Objecte interactuable mes proxim dins el radi.
    private GameObject currentTarget;

    void Start() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        UpdateTargetHighlight();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null) 
        {
            InteractWithTarget();
        }

        if (Input.GetKeyDown(KeyCode.Space) && carriedCube != null) DropCube();
    }

    void FixedUpdate()
    {
        // Converteix input 2D a moviment isometric en el pla XZ.
        Vector2 adjustedInput = ApplyControlModifiers(input);
        Vector3 moveDir = new Vector3(adjustedInput.x + adjustedInput.y, 0f, adjustedInput.y - adjustedInput.x).normalized;
        if (moveDir.sqrMagnitude > 0.01f) 
        {
            rb.MovePosition(rb.position + moveDir * GetEffectiveMoveSpeed() * Time.fixedDeltaTime);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    public void SetMoveSpeedMultiplier(string sourceId, float multiplier)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) return;

        moveSpeedModifiers[sourceId] = Mathf.Max(0f, multiplier);
    }

    public void ClearMoveSpeedMultiplier(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) return;

        moveSpeedModifiers.Remove(sourceId);
    }

    public void SetControlsReversed(string sourceId, bool reversed)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) return;

        if (reversed)
            reversedInputSources.Add(sourceId);
        else
            reversedInputSources.Remove(sourceId);
    }

    public void ClearControlsReversal(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId)) return;

        reversedInputSources.Remove(sourceId);
    }

    private Vector2 ApplyControlModifiers(Vector2 rawInput)
    {
        return reversedInputSources.Count > 0 ? -rawInput : rawInput;
    }

    private float GetEffectiveMoveSpeed()
    {
        float speedMultiplier = 1f;

        foreach (float modifier in moveSpeedModifiers.Values)
        {
            speedMultiplier *= modifier;
        }

        return moveSpeed * speedMultiplier;
    }

    private void UpdateTargetHighlight()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        GameObject closestObj = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            // Nomes es poden seleccionar cubs lliures o dispositius interactuables.
            bool isValidTarget = (carriedCube == null && hit.CompareTag("Cube")) || 
                                 hit.GetComponent<MemorySlot>() || 
                                 hit.GetComponent<Output>() || 
                                 hit.GetComponent<ALUController>() || 
                                 hit.GetComponent<BranchingButton>();

            if (isValidTarget)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist) { minDist = dist; closestObj = hit.gameObject; }
            }
        }

        if (closestObj != currentTarget)
        {
            if (currentTarget != null && currentTarget.TryGetComponent(out InteractableHighlight oldHl)) 
                oldHl.SetHighlight(false);
                
            currentTarget = closestObj;
            
            if (currentTarget != null && currentTarget.TryGetComponent(out InteractableHighlight newHl)) 
                newHl.SetHighlight(true);
        }
    }

    private void InteractWithTarget()
    {
        // Prioritza el tipus d'interaccio segons el component detectat.
        if (carriedCube == null && currentTarget.CompareTag("Cube")) { PickUpCube(currentTarget); return; }
        if (currentTarget.TryGetComponent(out MemorySlot memSlot)) { memSlot.HandleInteraction(this); return; }
        if (currentTarget.TryGetComponent(out Output outUnit)) { outUnit.HandleInteraction(this); return; }
        if (currentTarget.TryGetComponent(out ALUController alu)) { alu.HandleInteraction(this); return; }
        if (currentTarget.TryGetComponent(out BranchingButton btn)) { btn.PressButton(); return; }
    }

    private void PickUpCube(GameObject cube)
    {
        carriedCube = cube;
        // Ancorat al punt de ma i sense fisica mentre es transporta.
        carriedCube.transform.SetParent(carryPoint);
        carriedCube.transform.localPosition = new Vector3(-0.25f, 0.79f, 0.70f); 
        if (carriedCube.TryGetComponent(out Rigidbody cubeRb)) cubeRb.isKinematic = true;
    }

    public void DropCube()
    {
        if (carriedCube == null) return;
        carriedCube.transform.SetParent(null);
        if (carriedCube.TryGetComponent(out Rigidbody cubeRb)) cubeRb.isKinematic = false;
        carriedCube = null;
    }

    public void ConsumeCube()
    {
        Destroy(carriedCube);
        carriedCube = null;
    }
}