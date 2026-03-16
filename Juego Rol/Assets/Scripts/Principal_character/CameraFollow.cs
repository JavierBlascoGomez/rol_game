using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    
    public Vector2 minLimits;
    
    public Vector2 maxLimits;

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 position = new Vector3(Mathf.Clamp(player.position.x, minLimits.x, maxLimits.x),
            Mathf.Clamp(player.position.y, minLimits.y, maxLimits.y), transform.position.z);
        
        transform.position = position;
    }
}
