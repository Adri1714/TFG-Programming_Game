using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public Transform carryPoint;
    public float interactionRadius = 1.5f;
    public GameObject carriedCube;
    [SerializeField] private LayerMask interactableLayer = ~0;

    private Rigidbody rb;
    private Vector2 input;
    public bool IsMoving => input.sqrMagnitude > 0.01f;
    private readonly Dictionary<string, float> moveSpeedModifiers = new Dictionary<string, float>();
    private readonly HashSet<string> reversedInputSources = new HashSet<string>();
    public bool playerControlsReversed = false;

    private GameObject currentTarget;
    private CameraDizzy cameraDizzy;
    private InputEvents inputEvents;

    void Awake() => cameraDizzy = GetComponent<CameraDizzy>();
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputEvents = InputEvents.Instance;
        inputEvents.OnInteract += InteractWithTarget;
        inputEvents.OnDrop += DropCube;
    }

    void Update()
    {
        if (Time.timeScale == 0f) { input = Vector2.zero; return; }

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        UpdateTargetHighlight();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = Vector3.zero;  
        rb.angularVelocity = Vector3.zero;

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
        {
            reversedInputSources.Add(sourceId);
            playerControlsReversed = true;
        }
        else
        {
            reversedInputSources.Remove(sourceId);
            playerControlsReversed = false;
        }
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
            speedMultiplier *= modifier;
        return moveSpeed * speedMultiplier;
    }

    private void UpdateTargetHighlight()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);
        GameObject closestObj = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            bool isValidTarget = (carriedCube == null && hit.CompareTag("Cube")) ||
                                 hit.GetComponentInParent<IInteractable>() != null;

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
        if (currentTarget == null) return;

        if (carriedCube == null && currentTarget.CompareTag("Cube")) { PickUpCube(currentTarget); return; }

        IInteractable interactable = currentTarget.GetComponentInParent<IInteractable>();
        if (interactable != null) interactable.HandleInteraction(this);
    }

    private void PickUpCube(GameObject cube)
    {
        AudioManager.Play(l => l.pickup);
        carriedCube = cube;
        carriedCube.transform.SetParent(carryPoint);
        carriedCube.transform.localPosition = new Vector3(-0.25f, 0.79f, 0.70f);
        if (carriedCube.TryGetComponent(out Rigidbody cubeRb)) cubeRb.isKinematic = true;
    }

    public void DropCube()
    {
        if (carriedCube == null) return;
        AudioManager.Play(l => l.drop);
        carriedCube.transform.SetParent(null);
        if (carriedCube.TryGetComponent(out Rigidbody cubeRb)) cubeRb.isKinematic = false;
        carriedCube = null;
    }

    public void ConsumeCube()
    {
        Destroy(carriedCube);
        carriedCube = null;
    }
    private void OnDisable()
    {
        inputEvents.OnInteract -= InteractWithTarget;
        inputEvents.OnDrop -= DropCube;
    }
}
