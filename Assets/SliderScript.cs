using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    public Slider slider;          // The slider to read from
    public TMP_Text valueText;     // The TMP text to update

    void Start()
    {
        UpdateValueText(slider.value);
    }

    public void UpdateValueText(float value)
    {
        valueText.text = slider.value.ToString();
    }
}
