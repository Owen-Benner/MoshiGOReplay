using UnityEngine;
using System.Collections;
using System;

// https://forum.unity3d.com/threads/how-to-load-an-array-with-jsonutility.375735/

[Serializable]
public class SearchObj {
    public int objSpriteIndex; // Sprite of object to show
    public int objSpawnIndex; // Index for object to spawn at
}

[Serializable]
public class Scene {
    // How this scene will be used
    // Can be either: normal, explore
    public string mode;
    // Index of the object we want to show to the player
    // An object is a 2d rendering of a nondescript, abstract geometric model.
    // It will be displayed on a grey screen
    public int objShowIndex;
    public float showTime; // How long the object is on screen, seconds
    public float greyScreenTime; // How long the grey screen is shown, seconds
    public float greyScreenTimeTwo; // How long the second grey screen is shown (after a scene ends), seconds

    public int envIndex; // The environment the player is in
    public float envTime; // The time the player is in the environment
    public float envTimeTwo; // The time the player is in the environment after seeing object
    public int objSpawnIndex; // Index of position object spawns in
    public bool showObjAlways; // Will the object be billboarded from the start of the scene?
    public int playerSpawnIndex; // Index of position player spawns in
    public int landmarkSpawnIndex; // Index of position landmark spawns in
}

// Config is a list of scenes and the player's settings
[Serializable]
public class Config {
    // The list of scenes to execute
    public Scene[] scenes;
    // Global settings
    public float lookSlerpTime;
    public float pauseTime;
    public string phaseName;
    public string subjectName;
    public float playerMoveSpeed;
    public float objTriggerRadius;
    public string actionKey; // String of key for player to press to input an action. http://answers.unity3d.com/questions/762073/c-list-of-string-name-for-inputgetkeystring-name.html

    public bool replay;

    // Given the text of a JSON object, build the config class
    public static Config Create(string jsonString){
        return JsonUtility.FromJson<Config>(jsonString);
    }
}
