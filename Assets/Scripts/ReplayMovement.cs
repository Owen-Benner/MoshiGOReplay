using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayMovement : MonoBehaviour
{
    public Transform playPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveToFrame(float pose, float xPos, float zPos)
    {
        playPos.eulerAngles = new Vector3 (playPos.eulerAngles.x, pose,
            playPos.eulerAngles.z);
        playPos.position = new Vector3 (xPos, playPos.position.y, zPos);
    }
}
