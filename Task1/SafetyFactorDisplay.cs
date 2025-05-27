using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafetyFactorDisplay : MonoBehaviour {
    [Header("DASH Manager")]
    public DASHManager dash;                        // the DASHManager

    [Header("Slider")]
    public Slider slider;                           // the slider to control the safety factor

    [Header("Slider Value Label")]
    public TMP_Text label;                          // the text label that will show the slider value

    void Start(){
        slider.value = dash.safetyFactor;           // setting the slider value to the value of the safety factor in the DASHManager
        UpdateLabel(slider.value);                  // calling the UpdateLabel method

        slider.onValueChanged.AddListener(val => {  // when slider value changes, call the lambda in-line method below it
            dash.SetSafetyFactor(val);              // calls method in DASHManager to set the safety factor to the slider value
            UpdateLabel(val);                       // again calling the UpdateLabel method to set label value
        });
    }

    void UpdateLabel(float v){
        label.text = v.ToString("F2");              // sets the label text to the slider value with 2 decimal places using ToString
    }
}