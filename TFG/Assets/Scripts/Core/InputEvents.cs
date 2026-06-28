using UnityEngine;
using System;

public class InputEvents : MonoBehaviour
{
    public static InputEvents Instance { get; private set; }
    public event Action OnInteract;
    public event Action OnDrop;
    public event Action OnToggle;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) OnInteract?.Invoke();

        if (Input.GetKeyDown(KeyCode.Space)) OnDrop?.Invoke();

        if(Input.GetKeyDown(KeyCode.Escape)) OnToggle?.Invoke();
    }
}