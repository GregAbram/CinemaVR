using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class CheckBoxScript : MonoBehaviour
{
    private string value;

    public string GetValue()
    {
        return value;
    }

    // Start is called before the first frame update
    public void Setup(string title, ToggleGroup tg)
    {
        value = title;
        Toggle toggle = GetComponent<Toggle>();
        toggle.group = tg;
        Text text = toggle.GetComponentInChildren<Text>();
        text.text = title;
    }
}