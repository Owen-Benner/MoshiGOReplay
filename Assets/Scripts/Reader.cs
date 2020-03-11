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

    public struct Trial
    {
        public float goalx;
        public float goaly;
        public float pose;
        public float startx;
        public float starty;
        public float starttime;

        public Trial(string gx, string gy, string p, string sx, string sy,
            string st)
        {
            goalx = Single.Parse(gx);
            goaly = Single.Parse(gy);
            pose = Single.Parse(p);
            startx = Single.Parse(sx);
            starty = Single.Parse(sy);
            starttime = Single.Parse(st);
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
            trials = new List<Trial>();

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
                            else if(xmlReader.Name == "trial")
                            {
                                Trial newTrial = new Trial(
                                    xmlReader.GetAttribute("goalx"),
                                    xmlReader.GetAttribute("goaly"),
                                    xmlReader.GetAttribute("pose"),
                                    xmlReader.GetAttribute("startx"),
                                    xmlReader.GetAttribute("starty"),
                                    xmlReader.GetAttribute("starttime"));
                                trials.Add(newTrial);
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
