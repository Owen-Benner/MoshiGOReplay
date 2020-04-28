using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Reader : MonoBehaviour
{
	public Logic logic;
	public ReplayMovement reMove;
	public Timestamp stamp;

	public int actionFlag = 0; //1 if action, 2 if timeout

	public Eye lEye;
	public Eye rEye;

	private string outputName;

	private bool replaying = false;
	private bool stop = false;
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

		public string lefteyex;
		public string lefteyey;
		public string leftpupil;
		public string righteyex;
		public string righteyey;
		public string rightpupil;

		public string result;

		public Frame(string d, string p, string t, string _x, string _y,
			string lx, string ly, string lp, string rx, string ry, string rp,
			string r = null)
		{
			distance = Single.Parse(d);
			pose = Single.Parse(p);
			timestamp = Single.Parse(t);
			x = Single.Parse(_x);
			y = Single.Parse(_y);

			lefteyex = lx;
			lefteyey = ly;
			leftpupil = lp;
			righteyex = rx;
			righteyey = ry;
			rightpupil = rp;

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
	public List<Frame> framesgray;
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
			framesgray = new List<Frame>();
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
									xmlReader.GetAttribute("y"),

									xmlReader.GetAttribute("lefteyex"),
									xmlReader.GetAttribute("lefteyey"),
									xmlReader.GetAttribute("leftpupil"),
									xmlReader.GetAttribute("righteyex"),
									xmlReader.GetAttribute("righteyey"),
									xmlReader.GetAttribute("rightpupil"));

								frames.Add(newFrame);
							}
							else if(xmlReader.Name == "framegray")
							{
								Frame newFrame = new Frame("0", "0",
									xmlReader.GetAttribute("timestamp"), "0",
									"0", xmlReader.GetAttribute("lefteyex"),
									xmlReader.GetAttribute("lefteyey"),
									xmlReader.GetAttribute("leftpupil"),
									xmlReader.GetAttribute("righteyex"),
									xmlReader.GetAttribute("righteyey"),
									xmlReader.GetAttribute("rightpupil"));

								framesgray.Add(newFrame);
								//Debug.Log("framegray");
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
								
									xmlReader.GetAttribute("lefteyex"),
									xmlReader.GetAttribute("lefteyey"),
									xmlReader.GetAttribute("leftpupil"),
									xmlReader.GetAttribute("righteyex"),
									xmlReader.GetAttribute("righteyey"),
									xmlReader.GetAttribute("rightpupil"),

									xmlReader.GetAttribute("result"));
								frames.Add(newAction);
							}
							break;
					}
				}
			}
			/*
			foreach(frame frame in frames)
				Debug.Log(frame.timestamp);
			*/
		}
	}

	// update is called once per frame
	void Update()
	{
		//Debug.Log("Update");
		if(replaying)
		{
			//Debug.Log("Replaying");
			if(frames.Count == 0)
			{
				replaying = false;
				Debug.Log("end run");
			}
			else
			{
				float adjTime = Time.time - startTime + trials[0].starttime;
				//skip to last applicable frame.
				if(frames.Count > 1)
				{
					while(frames[1].timestamp < adjTime)
					{
						if(frames[0].result == null)
							frames.RemoveAt(0);
					}
				}
				//check to read next frame.
				Frame frame = frames[0];
				if(adjTime > frame.timestamp)
				{
					//Debug.Log("Going to frame");
					reMove.MoveToFrame(frame.pose, frame.x, frame.y, relOrigin);
					if(!Single.TryParse(frame.lefteyex, out lEye.xPos))
						lEye.xPos = -100f;
					if(!Single.TryParse(frame.lefteyey, out lEye.yPos))
						lEye.yPos = -100f;
					if(!Single.TryParse(frame.leftpupil, out lEye.diameter))
						lEye.diameter = 0f;
					if(!Single.TryParse(frame.righteyex, out rEye.xPos))
						rEye.xPos = -100f;
					if(!Single.TryParse(frame.righteyey, out rEye.yPos))
						rEye.yPos = -100f;
					if(!Single.TryParse(frame.rightpupil, out rEye.diameter))
						rEye.diameter = 0f;
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
					Debug.Log("end trial");
				}
			}
		}
		else if(!stop)
		{
			float adjTime = Time.time - startTime + trials[0].starttime;
			if(framesgray.Count > 1)
			{
				while(framesgray[1].timestamp < adjTime)
				{
					framesgray.RemoveAt(0);
				}
			}
			if(framesgray.Count < 1)
			{
				stop = true;
			}
			else
			{
				Frame frame = framesgray[0];
				if(adjTime > frame.timestamp)
				{
					if(!Single.TryParse(frame.lefteyex, out lEye.xPos))
						lEye.xPos = -100f;
					if(!Single.TryParse(frame.lefteyey, out lEye.yPos))
						lEye.yPos = -100f;
					if(!Single.TryParse(frame.leftpupil, out lEye.diameter))
						lEye.diameter = 0f;
					if(!Single.TryParse(frame.righteyex, out rEye.xPos))
						rEye.xPos = -100f;
					if(!Single.TryParse(frame.righteyey, out rEye.yPos))
						rEye.yPos = -100f;
					if(!Single.TryParse(frame.rightpupil, out rEye.diameter))
						rEye.diameter = 0f;
					framesgray.RemoveAt(0);
				}
			}
		}
	}

	public void StartTrial(Vector3 _relOrigin)
	{
		Debug.Log("starting trial");
		replaying = true;
		startTime = Time.time;
		reMove =
			GameObject.FindWithTag("Player").GetComponent<ReplayMovement>();
		relOrigin = _relOrigin;
		//stamp.AdjStartTime(frames[0].timestamp);
		lEye.gameObject.SetActive(true);
		rEye.gameObject.SetActive(true);
	}
}
