using UnityEngine;
using System.Collections;

public class Groundem : MonoBehaviour {
    void Start ()
    {
        var pos = transform.position;
        transform.position = Vector3.zero; // Hide player

        float height = GetComponent<Collider>().bounds.extents.z;
        RaycastHit output;
        Physics.Raycast(new Vector3(pos.x, pos.y - height - .1f, pos.z),
            Vector3.down, out output);
        transform.position = new Vector3(pos.x, pos.y - output.distance, pos.z);
    }
}
