using UnityEngine;
using UnityEngine.UI;

public class Timestamp : MonoBehaviour
{

    public float startTime;

    public Text text;

    private bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();

        //GameObject.DontDestroyOnLoad(transform.parent.gameObject);
        //SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = string.Format("{0:N4}", Time.time - startTime);

        //Check for pause input.
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(paused)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 1f;
                paused = false;
            }
            else
            {
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0f;
                paused = true;
            }
        }
    }

    public void AdjStartTime(float curTime)
    {
        startTime = Time.time - curTime;
    }

    /*
    void SwitchParent(Transform newParent)
    {
        transform.parent.gameObject.SetActive(false);
        transform.SetParent(newParent, false);
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        SwitchParent(GameObject.Find("Canvas_XMaze").transform);
    }
    */
}
