using UnityEngine;

public class MoveForTheObject : MonoBehaviour {

    public Transform obj;
    public float x, z;
    public float lastYPos;
    public bool movingX;

    private void Start ()
    {
        lastYPos = transform.position.y;
    }

    void Update () {
        if (movingX)
            transform.position = new Vector3(x, obj.position.y, obj.position.z);
        else
            transform.position = new Vector3(obj.position.x, obj.position.y, z);
    }
}
