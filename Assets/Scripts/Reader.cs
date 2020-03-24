using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Reader : MonoBehaviour
{
    public Logic logic;
    public ReplayMovement reMove;

    public int actionFlag = 0; //1 if action, 2 if timeout

    private string outputName;

    private bool replaying = false;
    private float startTime;

    private Vector3 relOrigin;

    //Create constructor accepting XmlReader
    public struct Frame
    {
        public float distance;
        public float pose;
        public float timestamp;
        public float x;
        public float y;
        public string result;

        public Frame(string d, string p, string t, string _x, string _y,
            string r = null)
        {
            distance = Single.Parse(d);
            pose = Single.Parse(p);
            timestamp = Single.Parse(t);
            x = Single.Parse(_x);
            y = Single.Parse(_y);
            result = r;
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

    public struct Done
    {
        public float time_elapsed;

        public Done(string t)
        {
            time_elapsed = Single.Parse(t);
        }
    };

    //Add done.

    public List<Frame> frames;
    public List<Trial> trials;
    public List<Done> dones;

    // Start is called before the first frame update
    void Start()
    {
        if(!logic.globalConfig.replay)
            enabled = false;
        else
        {
            frames = new List<Frame>();
            trials = new List<Trial>();
            dones = new List<Done>();

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
                            else if(xmlReader.Name == "done")
                            {
                                Done newDone = new Done(
                                    xmlReader.GetAttribute("time_elapsed"));
                                dones.Add(newDone);
                            }
                            else if(xmlReader.Name == "action")
                            {
                                Frame newAction = new Frame(
                                    xmlReader.GetAttribute("distance"),
                                    xmlReader.GetAttribute("pose"),
                                    xmlReader.GetAttribute("timestamp"),
                                    xmlReader.GetAttribute("x"),
                                    xmlReader.GetAttribute("y"),
                                    xmlReader.GetAttribute("result"));
                                frames.Add(newAction);
                            }
                            break;
                    }
                }
            }
            /*
            foreach(Frame frame in frames)
                Debug.Log(frame.timestamp);
            */
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(replaying)
        {
            if(frames.Count == 0)
            {
                replaying = false;
                Debug.Log("End run");
            }
            else
            {
                float adjTime = Time.time - startTime + trials[0].starttime;
                //Skip to last applicable frame.
                while(frames[1].timestamp < adjTime)
                {
                    if(frames[0].result == null)
                        frames.RemoveAt(0);
                }
                //Check to read next frame.
                Frame frame = frames[0];
                if(adjTime > frame.timestamp)
                {
                    reMove.MoveToFrame(frame.pose, frame.x, frame.y, relOrigin);
                    if(frames[0].result != null)
                    {
                        if(frames[0].result == "action")
                        {
                            actionFlag = 1;
                        }
                        else if(frames[0].result == "timeout")
                        {
                            actionFlag = 2;
                        }
                    }
                    frames.RemoveAt(0);
                }

                if(dones[0].time_elapsed < Time.time - startTime)
                {
                    replaying = false;
                    trials.RemoveAt(0);
                    dones.RemoveAt(0);
                    Debug.Log("End trial");
                }
            }
        }
    }

    public void StartTrial(Vector3 _relOrigin)
    {
        Debug.Log("Starting trial");
        replaying = true;
        startTime = Time.time;
        reMove =
            GameObject.FindWithTag("Player").GetComponent<ReplayMovement>();
        relOrigin = _relOrigin;
    }
}
