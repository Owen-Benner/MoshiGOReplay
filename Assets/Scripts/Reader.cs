using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Reader : MonoBehaviour
{
    public Logic logic;
    private string outputName;

    //Create constructor accepting XmlReader
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

            using(XmlReader xmlReader = XmlReader.Create(outputName))
            {
                while(xmlReader.Read())
                {
                    Debug.Log(xmlReader.NodeType);
                    switch(xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Debug.Log(xmlReader.Name);
                            for(int i = 0; i < xmlReader.AttributeCount; ++i)
                            {
                                Debug.Log(xmlReader.GetAttribute(i));
                            }
                            break;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
