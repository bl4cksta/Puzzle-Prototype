using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Toggle rotationToggle, colorToggle, preshowToggle;
    private void Start()
    {
        SetupSettings();
    }
    public void BtnSetRotationMode(bool mode)
    {
        PlayerPrefs.SetInt("UseRotation", mode ? 1 : 0);
    }
    public void BtnSetUseColorMode(bool mode)
    {
        PlayerPrefs.SetInt("UseColor", mode ? 1 : 0);
    }
    public void BtnSetPreshowMode(bool mode)
    {
        PlayerPrefs.SetInt("UsePreshow", mode ? 1 : 0);
    }
    void SetupSettings()
    {
        var rotation = PlayerPrefs.GetInt("UseRotation", 1);
        var color = PlayerPrefs.GetInt("UseColor", 1);
        var preshow = PlayerPrefs.GetInt("UsePreshow", 1);

        rotationToggle.isOn = rotation > 0;
        colorToggle.isOn = color > 0;
        preshowToggle.isOn = preshow > 0;
    }
}
