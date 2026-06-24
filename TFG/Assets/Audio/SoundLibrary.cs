using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Sound Effects - player/data")]
    public AudioClip pickup, drop, aluOperate, print, trash, declareVar, assignVar;
    public AudioClip footstep; 

    [Header("Sound Effects - State")]
    public AudioClip levelComplete, error;

    [Header("Sound Effects - Hazards")]
    public AudioClip shock, panic, dizzy;

    [Header("Sound Effects - UI")]
    public AudioClip buttonClick, buttonHover;
}
