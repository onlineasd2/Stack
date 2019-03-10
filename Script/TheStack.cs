using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TheStack : MonoBehaviour {
    
    public Material cupM;
    public Transform cutObjectT;
    public GameObject exampleCube;

    public Color32[] gameColors = new Color32[4];

    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5.0f;
    private const float ERROR_MARGIN = 0.1f;
    private const float STACK_BOUNDS_GAIN = 0.25f;
    private const int COMBO_START_GAIN = 3;

    private GameObject[] theStack;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int stackIndex;
    private int scoreCount = 0;
    private int combo = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;

    private bool isMovingOnX = true;
    private bool gameOver = false;

    private Vector3 desiredPostition;
    private Vector3 lastTilePosition;

    private void Start ()
    {
        theStack = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            theStack[i] = transform.GetChild(i).gameObject;

        stackIndex = transform.childCount - 1;
    }

    private void CreateRubble (Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody>();

        ColorMesh(go.GetComponent<MeshFilter>().mesh);
    }

    private void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                scoreCount++;
            }
            else
            {
                EndGame();
            }
        }
        MoveTile();

        // Move the stack
        transform.position = Vector3.Lerp(transform.position, desiredPostition, STACK_MOVING_SPEED * Time.deltaTime);
    }

    private void MoveTile ()
    {
        if (gameOver)
            return;

        tileTransition += Time.deltaTime * tileSpeed;
        if (isMovingOnX)
            theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
        else
            theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS_SIZE);
    }

    private void SpawnTile ()
    {
        lastTilePosition = theStack[stackIndex].transform.localPosition;
        stackIndex--;
        if (stackIndex < 0)
            stackIndex = transform.childCount - 1;

        desiredPostition = (Vector3.down) * scoreCount;
        theStack[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
        theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
    }

    private bool PlaceTile ()
    {
        Transform t = theStack[stackIndex].transform;

        // Cut
        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                // CUTE THE TILE
                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;

                // CUT
                GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(theStack[stackIndex]
                    , cutObjectT.GetChild(1).position
                    , -transform.right, cupM);

                pieces[0].GetComponent<MeshFilter>().mesh = exampleCube.GetComponent<MeshFilter>().mesh;

                if (!pieces[1].GetComponent<Rigidbody>())
                    pieces[1].AddComponent<Rigidbody>();
                // CUT

                Debug.Log("CUT");
                
                //if (CutObject())
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);



                /*
                CreateRubble
                (
                    new Vector3((t.position.x > 0)
                        ? t.position.x + (t.localScale.x / 2)
                        : t.position.x - (t.localScale.x / 2)
                        , t.position.y
                        , t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
                );
                */

                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
            }
            else
            { // Scaling tile
                if (combo > COMBO_START_GAIN)
                {
                    if (stackBounds.x > BOUNDS_SIZE)
                        stackBounds.x = BOUNDS_SIZE;

                    stackBounds.x += STACK_BOUNDS_GAIN;
                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                }

                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
        }
        else
        {
            float deltaZ = lastTilePosition.z - t.position.z;
            if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
            {
                // CUTE THE TILE
                combo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);
                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2;
                /*
                // CUT
                GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(theStack[stackIndex]
                    , new Vector3((lastTilePosition.x - lastTilePosition.x), lastTilePosition.y, lastTilePosition.z)
                    , -transform.right, cupM);

                if (!pieces[1].GetComponent<Rigidbody>())
                    pieces[1].AddComponent<Rigidbody>();
                // CUT

                Debug.Log("CUT");
                    */

                //if (CutObject())
                //t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                
                /*
                CreateRubble
                (
                    new Vector3(t.position.x
                        , t.position.y
                        , (t.position.z > 0)
                        ? t.position.z + (t.localScale.z / 2)
                        : t.position.z - (t.localScale.z / 2)),
                    new Vector3(Mathf.Abs(deltaZ), 1, t.localScale.z)
                );
                */

                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
            }
            else
            { // Scaling tile
                if (combo > COMBO_START_GAIN)
                {
                    stackBounds.y += STACK_BOUNDS_GAIN;
                    if (stackBounds.y > BOUNDS_SIZE)
                        stackBounds.y = BOUNDS_SIZE;

                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
                }
                combo++;
                t.localPosition = lastTilePosition + Vector3.up;
            }
        }

        // Change direction
        secondaryPosition = (isMovingOnX)
            ? t.localPosition.x
            : t.localPosition.z;
        isMovingOnX = !isMovingOnX;

        return true;
    }

    private bool CutObject ()
    {
        for (int i = 0; i < cutObjectT.childCount; i++)
            cutObjectT.GetComponentInChildren<ExampleUseof_MeshCut>().cut = true;

        return true;
    }

    private void ColorMesh(Mesh mesh)
    {
        Vector3[] verticles = mesh.vertices;
        Color32[] colors = new Color32[verticles.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);

        for (int i = 0; i < verticles.Length; i++)
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);

        mesh.colors32 = colors;
    }

    private Color32 Lerp4 (Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);
        else if (t < 0.66f)
            return Color.Lerp(b, c, (t - 0.66f) / 0.33f);
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

    private void EndGame()
    {
        Debug.Log("Lose");
        gameOver = true;
        theStack[stackIndex].AddComponent<Rigidbody>();
    }
}
