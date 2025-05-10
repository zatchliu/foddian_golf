using UnityEngine;

public class GolfClubScript : MonoBehaviour
{
    public Transform pivotPoint;
    public float radius = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        var direction = mousePosition - pivotPoint.position;
        if (direction.magnitude > radius)
        {
            direction.Normalize();
            direction *= radius;
            mousePosition = pivotPoint.position + direction;
        }

        GetComponent<Rigidbody2D>().MovePosition(mousePosition);
    }

}
