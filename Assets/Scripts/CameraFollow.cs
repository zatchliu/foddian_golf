using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float followSmoothness = 0.125f;

    [Header("Background Setup")]
    public string backgroundTag = "Course";  


    void LateUpdate()
    {

        if (target == null) return;
    
        Camera cam = Camera.main;
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth  = cam.aspect * halfHeight;

 
        GameObject[] bgGOs = GameObject.FindGameObjectsWithTag("Course");

        var firstRend = bgGOs[0].GetComponent<SpriteRenderer>();
        Bounds combinedBounds = firstRend.bounds;
        for (int i = 1; i < bgGOs.Length; i++)
        {
            var rend = bgGOs[i].GetComponent<SpriteRenderer>();
            combinedBounds.Encapsulate(rend.bounds);
        }

        // X Clamping Calculations
        float minX = combinedBounds.min.x + halfWidth;
        float maxX = combinedBounds.max.x - halfWidth;



        // Y Clamping Calculations
        float minY = combinedBounds.min.y + halfHeight;
        float maxY = combinedBounds.max.y - halfHeight;

        
        Vector3 desiredPos = target.position + offset;
        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmoothness);

    }
}
