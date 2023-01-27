using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

public class ButtonBoxScript : MonoBehaviour
{
    public GameObject checkBoxPrefab;
    private string title;
    private string current;
    private ToggleGroup toggleGroup;
    CinemaTime UI;

    public string GetTitle()
    {
        return title;
    }
    public string GetActive()
    {
        Toggle active = toggleGroup.GetFirstActiveToggle();
        CheckBoxScript cbs = active.GetComponent<CheckBoxScript>();
        return cbs.GetValue();
    }

    public void Setup(string t, JArray values, CinemaTime ui)
    {
        toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();
        title = t;
        UI = ui;

        foreach (Transform x in gameObject.transform)
        {
            if (x.gameObject.name == "Title")
            {
                TextMeshProUGUI text = x.GetComponentInChildren<TextMeshProUGUI>();
                text.text = title;
            }
            else if (x.gameObject.name == "Buttons")
            {   
                foreach (var value in values)
                {
                    GameObject go = Instantiate(checkBoxPrefab);
                    go.transform.SetParent(x.gameObject.transform, false);
                    CheckBoxScript cb = go.GetComponent<CheckBoxScript>();
                    cb.Setup((string)value, toggleGroup); 
                    Toggle toggle = go.GetComponent<Toggle>();
                    toggle.onValueChanged.AddListener((s) =>
                    {
                        if (s)
                        {
                            current = GetActive();
                            Debug.Log(title + ": " + current);
                            UI.UpdateDataset();
                        }
                    });
                }
            }
        }
    }
}