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

        // compute the base desired position
        Vector3 desiredPos = target.position + offset;

        // early-exit if no Course backgrounds are present
        GameObject[] bgGOs = GameObject.FindGameObjectsWithTag(backgroundTag);
        if (bgGOs.Length == 0)
        {
            // simple smooth follow with no clamping
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSmoothness);
            return;
        }

        // build combined bounds from all Course sprites
        var firstRend = bgGOs[0].GetComponent<SpriteRenderer>();
        Bounds combinedBounds = firstRend.bounds;
        for (int i = 1; i < bgGOs.Length; i++)
        {
            var rend = bgGOs[i].GetComponent<SpriteRenderer>();
            combinedBounds.Encapsulate(rend.bounds);
        }

        // camera half-extents
        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth  = cam.aspect * halfHeight;

        // clamp desiredPos to within those bounds
        float minX = combinedBounds.min.x + halfWidth;
        float maxX = combinedBounds.max.x - halfWidth;
        float minY = combinedBounds.min.y + halfHeight;
        float maxY = combinedBounds.max.y - halfHeight;

        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        // apply smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmoothness);
    }
}
