using UnityEngine;
using System.Collections;
using System.Xml;
using Tobii.Research;

// Logs to a file a configurable number of seconds
// What GO is subject to logging is settable, and
// is probably done by FPSChanger
public class Logger : MonoBehaviour {
	//
	//Highest level tag settings
	//
	
	//Version number
	public string VersionString = "1.0";

	//
	//Other settings
	//

	//File to output to
	public string XmlLogOutput = "temp.xml";

	//Log timer interval
	public float LogTimeInterval = 1f / 24f;

        //Enables all writing. Turn off for replay.
        public bool write = true;

        IEyeTracker eyeTracker;

	//
	//Private members
	//

	//Logging relevant state
	//

	//What GO should we log? This is the player and
	//is changed by the FPSChooser script
	private GameObject gameObjectToLog;

	//The goal the player is trying to move towards
	private Vector3 goalDestination;
	
	//The origin of each world
	private Vector3 relativeOrigin;

	//Other private state
	//

	//XmlWriter
	private XmlWriter m_writer;

        // Keep track of logging coroutine
        private IEnumerator logCoro;

	//Are we currently inside a trial element?
	private bool inTrial = false;

	//Time take to find object
	private float timeStart;

        private static GazeDataEventArgs gaze;

	//
	//Public methods
	//

	//Called to start the recording of a trial
	public void StartTrial(Vector3 destination, GameObject trackme, Vector3 relOrigin){
            if(write)
            {
		//Setup local state
		//
		gameObjectToLog = trackme;
		goalDestination = destination;
		relativeOrigin = relOrigin; // XXX DEBUG

		//Write a trial element
		//
		m_writer.WriteStartElement("trial");

		//Refer to an old log file for an idea
		//of what each printed thing means
		//
		m_writer.WriteAttributeString("goalx",
				(goalDestination.x - relativeOrigin.x).ToString());

		m_writer.WriteAttributeString("goaly",
				(goalDestination.z - relativeOrigin.z).ToString());

		m_writer.WriteAttributeString("pose",
				gameObjectToLog.transform.rotation.eulerAngles.y.ToString());

		m_writer.WriteAttributeString("startx",
				(gameObjectToLog.transform.position.x - relativeOrigin.x).ToString());

		m_writer.WriteAttributeString("starty",
				(gameObjectToLog.transform.position.z - relativeOrigin.z).ToString());

		m_writer.WriteAttributeString("starttime",
				(Time.time).ToString());

		//Setup timer; other state
		//
		inTrial = true;
                logCoro = WaitAndWriteFrame(LogTimeInterval);
                StartCoroutine(logCoro);
		timeStart = Time.time;
            }
	}

	//Ends a started trial
	public void EndTrial(int index = -1){
            if(write)
            {
		//Write the no bs element
		//
		m_writer.WriteStartElement("done");
		m_writer.WriteAttributeString("time_elapsed", (Time.time - timeStart).ToString());
		if(index != -1)
			m_writer.WriteAttributeString("index", index.ToString());
		m_writer.WriteEndElement();//End no bs element

		m_writer.WriteEndElement();//End trial element

                // We're out of the trial!
		inTrial = false;
                StopCoroutine(logCoro);
                logCoro = null;
            }
	}

    // Log when player presses action
    public void WriteAction(string actionval){
        WriteFrame("action", actionval);
    }

    // Phase controls
    // Phases wrap all trials

	public void StartPhase(string phase){
	    if(write)
                m_writer.WriteStartElement(phase);
	}
	
	public void EndPhase(){
            if(write)
		m_writer.WriteEndElement();
	}

	//
	//Helper methods
	//

	//Log the state of the current frame
	private void WriteFrame(string name, string extraAttrib=null){
            if(write)
            {
		//Writing this frame...
		m_writer.WriteStartElement(name);

		//This is our relevant data:
		//
		Transform t = gameObjectToLog.transform;
		m_writer.WriteAttributeString("distance", Vector3.Distance
                    (t.position, goalDestination).ToString());
		m_writer.WriteAttributeString("pose",
                    t.rotation.eulerAngles.y.ToString());
		m_writer.WriteAttributeString("timestamp",
                    Time.time.ToString());
		m_writer.WriteAttributeString("x", (t.position.x
                    - relativeOrigin.x).ToString());
		m_writer.WriteAttributeString("y", (t.position.z
                    - relativeOrigin.z).ToString());

                //Gaze data. 
                try
                {
                    GazePoint point = gaze.LeftEye.GazePoint;
                    if(point.Validity == Validity.Valid)
                    {
                        m_writer.WriteAttributeString("lefteyex",
                            point.PositionOnDisplayArea.X.ToString());
                        m_writer.WriteAttributeString("lefteyey",
                            point.PositionOnDisplayArea.Y.ToString());
                    }
                    else
                    {
                        m_writer.WriteAttributeString("lefteyex", "invalid");
                        m_writer.WriteAttributeString("lefteyey", "invalid");
                    }

                    PupilData pupil = gaze.LeftEye.Pupil;
                    if(pupil.Validity == Validity.Valid)
                    {
                        m_writer.WriteAttributeString("leftpupil",
                            pupil.PupilDiameter.ToString());
                    }
                    else
                    {
                        m_writer.WriteAttributeString("leftpupil", "invalid");
                    } 

                    if(extraAttrib != null){
                        m_writer.WriteAttributeString("result", extraAttrib);
                    }

                    point = gaze.RightEye.GazePoint;
                    if(point.Validity == Validity.Valid)
                    {
                        m_writer.WriteAttributeString("righteyex",
                            point.PositionOnDisplayArea.X.ToString());
                        m_writer.WriteAttributeString("righteyey",
                            point.PositionOnDisplayArea.Y.ToString());
                    }
                    else
                    {
                        m_writer.WriteAttributeString("righteyex", "invalid");
                        m_writer.WriteAttributeString("righteyey", "invalid");
                    }

                    pupil = gaze.RightEye.Pupil;
                    if(pupil.Validity == Validity.Valid)
                    {
                        m_writer.WriteAttributeString("rightpupil",
                            pupil.PupilDiameter.ToString());
                    }
                    else
                    {
                        m_writer.WriteAttributeString("rightpupil", "invalid");
                    } 
                }
                catch{}

                if(extraAttrib != null){
                    m_writer.WriteAttributeString("result", extraAttrib);
                }

                //Done!
                m_writer.WriteEndElement();
            }
        }

	public void InitLogger(string subjectName){
            if(write)
            {
		//Xml
		//

		//Setup XmlWriter with indenting enabled (uses hot C# syntax for Object Initializer)
		//TODO Try/catch/finally, or just crash
		m_writer = XmlWriter.Create(XmlLogOutput, new XmlWriterSettings(){Indent = true});

		//Start our document
		m_writer.WriteStartDocument();

		//Add some high level information
		m_writer.WriteStartElement("run");
		m_writer.WriteAttributeString("subject", subjectName);
		m_writer.WriteAttributeString("version", VersionString);
            }
	}

	public void LogDebug(string s){
            if(write)
            {
		m_writer.WriteStartElement("run");
		m_writer.WriteEndElement();
            }
	}

	//
	//Unity callbacks
	//

    public IEnumerator WaitAndWriteFrame(float waitTime)
    {
        float nextTime = Time.time + waitTime;
        while(true)
        {
            while(Time.time < nextTime)
                yield return null;
            while(Time.time >= nextTime)
                nextTime += waitTime;
            WriteFrame("frame");
        }
    }

    //TODO exceptions/using statements
    //Close our file and that kind of thing
    void OnDestroy(){
        if(write)
        {
            if(inTrial)
                    EndTrial();

            //End high level information
            m_writer.WriteEndElement();

            //End our document
            m_writer.WriteEndDocument();

            //Close file
            m_writer.Close();
        }
    }

    private void Start()
    {
        try
        {
            eyeTracker = EyeTrackingOperations.FindAllEyeTrackers()[0];
            eyeTracker.GazeDataReceived += EyeTracker_GazeDataReceived;
        }
        catch
        {
            Debug.LogError("Eye tracker not found!");
        } 
    }

    private void EyeTracker_GazeDataReceived(object sender, GazeDataEventArgs e)
    {
        gaze = e;
        Debug.Log("Recieved gaze data.");
    } 

    private void OnApplicationQuit()
    {
        try
        {
            Debug.Log("Terminating eye tracker operation.");
            eyeTracker.GazeDataReceived -= EyeTracker_GazeDataReceived;
            EyeTrackingOperations.Terminate();
        }
        catch{}
    } 

}

