using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.Characters.FirstPerson;

public class PlayerAction : MonoBehaviour {

    private Config globalConfig = null; // A reference to the global config object, which holds keybinding configurations and other things

    private Transform child;

    private FirstPersonController fpscont; // Enable/Disable input
    private CharacterController charcont; // Freeze/Unfreeze player

    private ReplayMovement reMove;

    void Awake(){
        globalConfig = GameObject.Find("Logic").GetComponent<Logic>().globalConfig;
        child = transform.GetChild(0);
        GameObject go = transform.gameObject;
        fpscont = (FirstPersonController)go.GetComponent<FirstPersonController>();
        charcont = (CharacterController)go.GetComponent<CharacterController>();
        reMove = GetComponent<ReplayMovement>();
    }

    // Coroutines
    public IEnumerator PlayerLookTowards(){
        Vector3 goalpos = GameObject.FindWithTag("GoalTrigger").transform.position - transform.position;
        Quaternion goalrot = Quaternion.LookRotation(goalpos);
        Quaternion goalrotflat = Quaternion.Euler(0, goalrot.eulerAngles.y, 0);

        Quaternion initrot = transform.rotation;
        float curtime = Time.time;
        while(Time.time - curtime < globalConfig.lookSlerpTime){
            float prop = (Time.time - curtime) / globalConfig.lookSlerpTime;
            transform.rotation = Quaternion.Slerp(initrot, goalrotflat, prop);
            child.rotation = Quaternion.Slerp(initrot, goalrotflat, prop);
            yield return null;
        }
    }

    // Messages

    public void DisableInput(){
        fpscont.enabled = false;
    }

    public void EnableInput(){
        if(!globalConfig.replay)
            fpscont.enabled = true;
    }

    public void FreezePlayer(){
        charcont.enabled = false;
    }

    public void UnFreezePlayer(){
        charcont.enabled = true;
    }

    public void DisableReplay()
    {
        reMove.enabled = false;
    }

    public void EnableReplay()
    {
        reMove.enabled = true;
    }
}
