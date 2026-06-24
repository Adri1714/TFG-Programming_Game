using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [SerializeField] private bool useGameMusic;
    private void Start() => AudioManager.Music(l => useGameMusic ? l.gameMusic : l.menuMusic);
}