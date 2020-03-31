using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Eye : MonoBehaviour
{
    public float xPos;
    public float yPos;
    public float diameter;

    public int xRes;
    public int yRes;

    public CanvasScaler scale;

    private Transform pupil;

    void Awake()
    {
        scale = transform.parent.GetComponent<CanvasScaler>();
        scale.referenceResolution = new Vector2(xRes, yRes);
    }

    // Start is called before the first frame update
    void Start()
    {
        pupil = transform.GetChild(0);

        //SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    // Update is called once per frame
    void Update()
    {
        float xVal = xPos * xRes - xRes / 2;
        float yVal = -(yPos * yRes - yRes / 2);
        transform.localPosition = new Vector3(xVal, yVal,
            transform.localPosition.z);

        pupil.localScale = new Vector3(diameter, diameter, pupil.localScale.z);
    }

    /*
    void SwitchParent(Transform newParent)
    {
        transform.SetParent(newParent, false);
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        SwitchParent(GameObject.Find("Canvas_XMaze").transform);

        scale = transform.parent.GetComponent<CanvasScaler>();
        scale.referenceResolution = new Vector2(xRes, yRes);
    }
    */

}
