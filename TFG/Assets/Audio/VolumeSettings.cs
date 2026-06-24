using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private Slider masterSlider, musicSlider, sfxSlider;

    private void OnEnable()
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        Bind(masterSlider, am.GetMaster(), am.SetMasterVolume);
        Bind(musicSlider,  am.GetMusic(),  am.SetMusicVolume);
        Bind(sfxSlider,    am.GetSfx(),     am.SetSfxVolume);
    }

    private void OnDisable()
    {
        if (masterSlider) masterSlider.onValueChanged.RemoveAllListeners();
        if (musicSlider)  musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider)    sfxSlider.onValueChanged.RemoveAllListeners();
    }

    private void Bind(Slider s, float value, UnityEngine.Events.UnityAction<float> setter)
    {
        if (s == null) return;
        s.minValue = 0f; s.maxValue = 1f;
        s.SetValueWithoutNotify(value);
        s.onValueChanged.AddListener(setter);
    }
}