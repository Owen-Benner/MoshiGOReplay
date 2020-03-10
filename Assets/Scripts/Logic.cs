using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System;
using System.IO;
using System.Text;
using UnityStandardAssets.Characters.FirstPerson;

public class Logic : MonoBehaviour {
    public GameObject player;

    private float IntroGreyScreenTime = 4.0f;

    public GameObject Environments = null;
    public GameObject CanvasCoord = null;

    public Config globalConfig = null; // Global settings, like actionKey

    public string outputName = "temp.xml";

    private bool fplayerfoundtarget = false;

    private void ResetCallbacks(){
        fplayerfoundtarget = false;
    }

    // Callback given to Triggers / Objects to let our guy know we're done
    private void TriggerCallback(){
        fplayerfoundtarget = true;
    }

    private IEnumerator OnFindTarget(GameObject player){
        print("GOOD JOB U FOUND TARGET");
        yield return new WaitForSeconds(globalConfig.pauseTime);
    }

    // Helper
    private GameObject GetEnvGO(GameObject environments, int envIndex){
        return environments.transform.GetChild(envIndex).gameObject;
    }

    // objShowIndex: index of object to show
    // showTime: time we show the object
    // greyScreenTime: time we show the plus
    private IEnumerator ShowGrayScreen(int objShowIndex, float showTime, float greyScreenTime){
        if(showTime <= 0.0f && greyScreenTime <= 0.0f){
            // End immediately
            yield break;
        }

        CanvasCoord.SendMessage("ShowGray");
        print("ShowGrayScreen(): Enabled grayscreen");

        if(showTime > 0.0f){
            CanvasCoord.SendMessage("ShowImage", objShowIndex);
            print(String.Format("ShowGrayScreen(): Enabled Image {0}", objShowIndex));

            yield return new WaitForSeconds(showTime);

            CanvasCoord.SendMessage("HideImage");
            print(String.Format("ShowGrayScreen(): Disabled Image {0}", objShowIndex));
        }

        if(greyScreenTime > 0.0f){
            CanvasCoord.SendMessage("ShowPlus");

            yield return new WaitForSeconds(greyScreenTime);

            CanvasCoord.SendMessage("HidePlus");
        }

        CanvasCoord.SendMessage("HideGray");
        print("ShowGrayScreen(): Disabled grayscreen");
    }

    //
    // Scene logics
    //

    public IEnumerator RunNormalScene(Scene s, Logger logger){
        print("RunNormalScene(): Starting");

        // Show GrayScreen
        yield return StartCoroutine(ShowGrayScreen(s.objShowIndex, s.showTime, s.greyScreenTime));

        // Env we'll be sending message to
        GameObject curenv = GetEnvGO(Environments, s.envIndex);

        // Setup player
        if(s.playerSpawnIndex >= 0){
            // Player index of -1 or less implies we dont respawn player. Used for searchfind.
            Environments.BroadcastMessage("RemovePlayer");
            curenv.BroadcastMessage("SpawnPlayerAtIndex", s.playerSpawnIndex);
        }
        GameObject player = GameObject.FindWithTag("Player");
        player.SendMessage("EnableInput");

        // Setup trigger object
        curenv.BroadcastMessage("ActivateObjTriggerAtIndex",
                new ObjSpawner.TriggerInfo(s.objSpawnIndex, TriggerCallback, s.objShowIndex, globalConfig.objTriggerRadius));
        if(s.showObjAlways)
            curenv.BroadcastMessage("ShowSelf");

        Vector3 objpos = GameObject.FindWithTag("GoalTrigger").transform.position;

        // Setup Landmark
        curenv.BroadcastMessage("ShowLandmark", s.landmarkSpawnIndex);

        // Setup logger, using environment info component
        EnvInfo envinfo = (EnvInfo)curenv.GetComponent<EnvInfo>();
        logger.StartTrial(envinfo.GetActiveTriggerObj().transform.position, player, envinfo.GetOrigin());

        // Wait for player to find target
        float curtime = Time.time;
        yield return new WaitUntil(() =>
                (s.showObjAlways ?
                 fplayerfoundtarget :
                 (Input.GetKeyDown(globalConfig.actionKey) || Time.time - curtime >= s.envTime)));

        bool ftimedout = Time.time - curtime >= s.envTime;

        // Log player doing action/timing out
        logger.WriteAction(ftimedout ? "timeout" : "action");

        // Freeze player controls
        player.SendMessage("DisableInput");

        // Check if player found target after the press
        bool ffoundtarget = !(Time.time - curtime >= s.envTime) &&
            (s.showObjAlways ?
                fplayerfoundtarget :
                (globalConfig.objTriggerRadius + player.GetComponent<CharacterController>().radius) >=
                 Vector2.Distance(Helper.ToVector2(objpos),
                                  Helper.ToVector2(player.transform.position)));

        print(String.Format("RunScene(): ffoundtarget: '{0}'", ffoundtarget));

        PlayerAction playerAction = (PlayerAction)player.GetComponent<PlayerAction>();

        if(ffoundtarget){
            print("You found the target without having to be shown it!");
            curenv.BroadcastMessage("ShowSelf");

            // Turn player towards object
            if(!s.showObjAlways)
                yield return StartCoroutine(playerAction.PlayerLookTowards());
        }else{
            print("You didn't find the target in time. Let me show it to you...");

            // Turn the target on! Show the billboarded sprite!
            curenv.BroadcastMessage("ShowSelf");

            // Turn player towards object
            yield return StartCoroutine(playerAction.PlayerLookTowards());

            // Reset target
            ResetCallbacks();

            // Unfreeze controls
            player.SendMessage("EnableInput");

            print("...now go find the target!");

            float curtimetwo = Time.time;
            yield return new WaitUntil(() => fplayerfoundtarget || Time.time - curtimetwo >= s.envTimeTwo);

            // Freeze player controls
            player.SendMessage("DisableInput");
        }

        if(!s.showObjAlways)
            yield return StartCoroutine(OnFindTarget(player));

        curenv.BroadcastMessage("HideSelf");

        logger.EndTrial();

        curenv.BroadcastMessage("DeactiveateTriggers");
        curenv.BroadcastMessage("HideLandmark");

        ResetCallbacks();

        yield return StartCoroutine(ShowGrayScreen(-1, 0.0f, s.greyScreenTimeTwo));

        print("Scene(): Done");
    }

    // Placed in environment where they can explore for 2-3 mins, no objects.
    public IEnumerator RunExploreScene(Scene s, Logger logger){
        print("ExploreScene(): Starting");

        // Doesn't have GrayScreen

        GameObject curenv = GetEnvGO(Environments, s.envIndex);
        curenv.BroadcastMessage("SpawnPlayerAtIndex", s.playerSpawnIndex);

        // Get player reference
        GameObject player = GameObject.FindWithTag("Player");

        // Setup logger, using environment info component
        EnvInfo envinfo = (EnvInfo)curenv.GetComponent<EnvInfo>();
        logger.StartTrial(Vector3.zero, player, envinfo.GetOrigin()); // No destination in explore mode

        yield return new WaitForSeconds(s.envTime);

        logger.EndTrial();

        curenv.BroadcastMessage("RemovePlayer");

        print("ExploreScene(): Done");
    }

    // In environment, obj in the world, go get it, repeat
    public IEnumerator RunScene(Scene s, Logger logger){
        switch(s.mode){
            case "normal":
                yield return StartCoroutine(RunNormalScene(s, logger));
                break;
            case "explore":
                yield return StartCoroutine(RunExploreScene(s, logger));
                break;
            default:
                System.Diagnostics.Debug.Assert (false, String.Format ("scene mode is invalid: '{0}'", s.mode));
                break;
        }
    }

    public IEnumerator RunAllScenes(IEnumerable<Scene> scenes, Logger logger){
        // Show the intro screen first
        yield return StartCoroutine(IntroGreyScreen(IntroGreyScreenTime));

        logger.StartPhase(globalConfig.phaseName);

        int counter = 0;
        foreach(Scene s in scenes){
            print(String.Format("RunAllScenes(): Running scene number: {0}", counter));

            yield return StartCoroutine(RunScene(s, logger));

            print(String.Format("RunAllScenes(): Done running scene number: {0}", counter));
            counter += 1;
        }

        logger.EndPhase();

        print("RunAllScenes(): Done running all scenes!");

        Application.Quit();
    }

    IEnumerator IntroGreyScreen(float plusTime){
        CanvasCoord.SendMessage("ShowGray");
        print("IntroGreyScreen(): Enabled grayscreen");

        CanvasCoord.SendMessage("ShowPlus");

        print("IntroGreyScreen(): Waiting for '5' key press...");
        yield return new WaitUntil(() => Input.GetKeyDown("5"));
        print("IntroGreyScreen(): ...Got it!");

        yield return new WaitForSeconds(plusTime);
        CanvasCoord.SendMessage("HidePlus");

        CanvasCoord.SendMessage("HideGray");
        print("IntroGreyScreen(): Disabled grayscreen");
    }

    void Awake(){
        // Read Json file to configure stuff

        string jsonpath = @"config.json";
        if(!File.Exists(jsonpath)){
            Debug.LogError(String.Format("Couldn't load config file at: {0}", jsonpath));
            Debug.Assert(false);
            Application.Quit();
        }

        string configjson = File.ReadAllText(jsonpath);
        Config config = Config.Create(configjson);
        globalConfig = config;

        if(globalConfig.replay == false)
            Debug.Log("Not a replay");
        if(globalConfig.replay == true)
            Debug.Log("Replay");
    }

    void Start()
    {
        // Setup logger
        Logger logger = GetComponent<Logger>();
        if(globalConfig.replay)
        {
            logger.write = false;
            player.GetComponent<FirstPersonController>().enabled = false;
            player.GetComponent<ReplayMovement>().enabled = true;
        }

        logger.InitLogger(globalConfig.subjectName);
        logger.XmlLogOutput = outputName;

        StartCoroutine(RunAllScenes(globalConfig.scenes, logger));
    }
}
