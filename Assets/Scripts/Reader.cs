using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Reader : MonoBehaviour
{
    public Logic logic;
    private string outputName;

    private struct Frame
    {
        float distance;
        float pose;
        float timestamp;
        float x;
        float y;
    };

    // Start is called before the first frame update
    void Start()
    {
        if(!logic.globalConfig.replay)
            enabled = false;
        else
        {
            Debug.Log("Read log.");
            outputName = logic.outputName;

            using(XmlReader xmlr = XmlReader.Create(outputName))
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
