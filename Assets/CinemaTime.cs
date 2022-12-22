using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;



public class CinemaTime : MonoBehaviour
{
    public GameObject UI;
    public GameObject sliderPrefab;
    public GameObject buttonBoxPrefab;
    public SliderScript[] sliderArray;
    public ButtonBoxScript[] buttonBoxArray;

    public string SendGet(string s)
    {
        byte[] reply = new byte[1024];
        Debug.Log("SendGet: " + s);

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

    public string GetInfo()
    {
        return SendGet("info");

    }

    void Start()
    {
        string info = GetInfo(); 
        
        JObject desc = JObject.Parse(info);

        JArray sliders = (JArray)desc["sliders"];
        sliderArray = new SliderScript[sliders.Count];
        int indx = 0;
        foreach (var slider in sliders)
        {
            GameObject go = Instantiate(sliderPrefab);
            go.transform.SetParent(UI.transform, false);
            SliderScript sscript = go.GetComponent<SliderScript>();
            sscript.Setup((string)slider["name"], (int)slider["min"], (int)slider["max"], this);
            sliderArray[indx++] = sscript;
        }

        JArray bboxes = (JArray)desc["radio_buttons"];
        buttonBoxArray = new ButtonBoxScript[bboxes.Count];
        indx = 0;
        foreach (var bbox in bboxes)
        {
            GameObject go = Instantiate(buttonBoxPrefab);
            go.transform.SetParent(UI.transform, false);
            ButtonBoxScript bbscript = go.GetComponent<ButtonBoxScript>();
            bbscript.Setup((string)bbox["name"], (JArray)bbox["values"], this);
            buttonBoxArray[indx++] = bbscript;
        }
    }

    public void UpdateDataset()
    {
        JObject rq = new JObject();
        foreach (ButtonBoxScript bb in buttonBoxArray)
        {
            rq.Add(bb.GetTitle(), bb.GetActive());
        }
        foreach (SliderScript s in sliderArray)
        {
            rq.Add(s.GetTitle(), s.GetValue());
        }

        Debug.Log("result: " + rq.ToString());
        SendGet(rq.ToString());
    }
} 
