using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    private int min;
    private int max;
    private int numberOfTimesteps;
    private string title;
    private Button prev;
    private Button next;
    private Slider slider;
    private int current = 0;
    CinemaTime UI;

    public string GetTitle()
    {
        return title;
    }

    public int GetValue()
    {
        return current;
    }

    public void Setup(string tt, int m, int M, CinemaTime ui)
    {
        min = m;
        max = M;
        UI = ui;
        title = tt;

        numberOfTimesteps = (M - m) + 1;

        foreach (Transform t in gameObject.transform)
        {

            if (t.gameObject.name == "Title")
            {
                TextMeshProUGUI text = t.GetComponentInChildren<TextMeshProUGUI>();
                text.text = title;
            }
            else if (t.gameObject.name == "Next")
            {
                next = t.GetComponent<Button>();
                next.onClick.AddListener(delegate { Next(); });
            }
            else if (t.gameObject.name == "Prev")
            {
                prev = t.GetComponent<Button>(); 
                prev.onClick.AddListener(delegate { Prev(); });
            }
            else if (t.gameObject.name == "Slider")
            {
                slider = t.GetComponent<Slider>(); 
                slider.onValueChanged.AddListener(delegate { Slider(); });
            }
        }
    }

    void Next()
    {
        current = current + 1;
        if (current == numberOfTimesteps)
            current = 0;
        float v = ((float)current) / (numberOfTimesteps - 1);
        slider.SetValueWithoutNotify(v); 
        UI.UpdateDataset();
    }

    void Prev()
    {
        current = current - 1;
        if (current == 0)
            current = numberOfTimesteps - 1;
        float v = ((float)current) / (numberOfTimesteps - 1);
        slider.SetValueWithoutNotify(v);
        UI.UpdateDataset();
    }

    void Slider()
    {
        float v = slider.value;
        float fc = (v * (numberOfTimesteps - 1)) - (1 / (2 * (numberOfTimesteps - 1)));
        if ((int)fc != current)
        {
            current = (int)fc;
            UI.UpdateDataset();
        }
    }
}
