using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Reader : MonoBehaviour
{
    public Logic logic;
    private string outputName;

    //Create constructor accepting XmlReader
    public struct Frame
    {
        public float distance;
        public float pose;
        public float timestamp;
        public float x;
        public float y;

        public Frame(string d, string p, string t, string _x, string _y)
        {
            distance = Single.Parse(d);
            pose = Single.Parse(p);
            timestamp = Single.Parse(t);
            x = Single.Parse(_x);
            y = Single.Parse(_y);
        }
    };

    public List<Frame> frames;

    // Start is called before the first frame update
    void Start()
    {
        if(!logic.globalConfig.replay)
            enabled = false;
        else
        {
            frames = new List<Frame>();
            outputName = logic.outputName;

            using(XmlReader xmlReader = XmlReader.Create(outputName))
            {
                while(xmlReader.Read())
                {
                    switch(xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if(xmlReader.Name == "frame")
                            {
                                Frame newFrame = new Frame(
                                    xmlReader.GetAttribute("distance"),
                                    xmlReader.GetAttribute("pose"),
                                    xmlReader.GetAttribute("timestamp"),
                                    xmlReader.GetAttribute("x"),
                                    xmlReader.GetAttribute("y"));
                                frames.Add(newFrame);
                            }
                            break;
                    }
                }
            }

            //foreach(Frame frame in frames)
                //Debug.Log(frame.timestamp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
