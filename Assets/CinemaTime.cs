using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class CinemaTime : MonoBehaviour
{
    public Slider timeSlider;
    public Button prevButton;
    public Button nextButton;
    public GameObject buttonBox;
    public GameObject datasetPrefab;
    
    ToggleGroup toggleGroup;
 
    private int numberOfTimesteps = 0;

    private int current = -1;

    public Toggle currentSelection { get { return toggleGroup.ActiveToggles().FirstOrDefault(); } }

    public string SendGet(string s)
    {
        byte[] reply = new byte[1024];
 
        try
        {
            IPHostEntry host = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1901);

            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(remoteEP);
            byte[] msg = Encoding.ASCII.GetBytes(s);
            sender.Send(msg);
            int n = sender.Receive(reply);
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
        catch (Exception e)
        {
            reply = Encoding.ASCII.GetBytes("error");
            Debug.Log(e.ToString());
        }
        return Encoding.Default.GetString(reply);
    }

    public void SendRequest(int seq)
    {
        ToggleGroup tg = buttonBox.GetComponent<ToggleGroup>();
        string dset = "";

        foreach (Toggle t in tg.ActiveToggles())
            if (t.isOn)
            {
                dset = t.GetComponentInChildren<Text>().text;
                break;
            }

        if (dset != "Toggle")
        {
            string reqst = String.Format("{0}:{1}", dset, seq);
            Debug.Log("REQUEST:" + reqst);
            SendGet(reqst);
        }
    }


    public void chooseDB(bool b)
    {
        if (b)
            SendRequest(current);
    }

    public string GetInfo()
    {
        return SendGet("info");

    }

    void Start()
    {
        timeSlider.onValueChanged.AddListener(delegate { SliderUpdate(); });
        prevButton.onClick.AddListener(delegate { Prev(); });
        nextButton.onClick.AddListener(delegate { Next(); });
        
        string info = GetInfo();
        string[] list = info.TrimEnd('\0').Split(',');
        Debug.Log(info);

        toggleGroup = buttonBox.GetComponent<ToggleGroup>();
        numberOfTimesteps = Int32.Parse(list[0]);

        foreach (string dset in list[1..^0])
        {
            GameObject go = Instantiate(datasetPrefab);
            Toggle toggle = go.GetComponent<Toggle>();
            go.name = dset;
            toggle.transform.SetParent(buttonBox.transform, false);
            toggle.group = toggleGroup;
            Text txt = toggle.GetComponentInChildren<Text>();
            txt.text = dset;
  
            toggle.onValueChanged.AddListener((bool on) => { chooseDB(on); });
        }

        current = 0;
        timeSlider.SetValueWithoutNotify(0);
    }

    void SetTimestep(int c)
    {
        current = c;
        SendRequest(current);
    }

    public void SliderUpdate()
    {
        float v = timeSlider.value;
        float fc = (v * (numberOfTimesteps - 1)) - (1 / (2 * (numberOfTimesteps - 1)));
  
        int c = (int)fc;
        if (current != c)
            SetTimestep(c);
    }

    public void Next()
    {
        int c = current + 1;
        if (c == numberOfTimesteps)
            c = 0;
        float v = ((float)c) / (numberOfTimesteps - 1);
        timeSlider.SetValueWithoutNotify(v);
        SetTimestep(c);
    }

    public void Prev()
    {
        int c = current - 1;
        if (c == -1)
            c = numberOfTimesteps - 1;
        float v = ((float)c) / (numberOfTimesteps - 1);
        timeSlider.SetValueWithoutNotify(v);
        SetTimestep(c);
    }
}
