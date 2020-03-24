using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayMovement : MonoBehaviour
{
    private Transform playPos;

    // Start is called before the first frame update
    void Start()
    {
        playPos = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveToFrame(float pose, float xPos, float zPos,
        Vector3 relOrigin)
    {
        playPos.eulerAngles = new Vector3 (playPos.eulerAngles.x, pose,
            playPos.eulerAngles.z);
        playPos.position = new Vector3 (xPos + relOrigin.x, playPos.position.y,
            zPos + relOrigin.z);
    }
}
