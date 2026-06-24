using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    public enum Channel { Master, Music, Sfx }
    [SerializeField] private Channel channel = Channel.Master;

    private Slider slider;

    private void Awake() => slider = GetComponent<Slider>();

    private void OnEnable()
    {
        var am = AudioManager.Instance;
        if (am != null)
            slider.SetValueWithoutNotify(GetCurrent(am) * slider.maxValue);
        slider.onValueChanged.AddListener(Apply);
    }

    private void OnDisable() => slider.onValueChanged.RemoveListener(Apply);

    private float GetCurrent(AudioManager am) => channel switch
    {
        Channel.Master => am.GetMaster(),
        Channel.Music  => am.GetMusic(),
        _              => am.GetSfx()
    };

    private void Apply(float v)
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        float norm = slider.maxValue > 0f ? v / slider.maxValue : v;
        switch (channel)
        {
            case Channel.Master: am.SetMasterVolume(norm); break;
            case Channel.Music:  am.SetMusicVolume(norm);  break;
            case Channel.Sfx:    am.SetSfxVolume(norm);    break;
        }
    }
}