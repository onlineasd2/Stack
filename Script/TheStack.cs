using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TheStack : MonoBehaviour {
    
    public Material cupM;
    public Transform cutObjectT;
    public GameObject exampleCube;
    public Transform target;

    public GameObject lastGameMesh;
    public List<Material> lastMaterialObject;
    public List<GameObject> listFakeBlocks;

    public Color32[] gameColors = new Color32[4];

    private const float BOUNDS_SIZE = 3.5f;
    private const float STACK_MOVING_SPEED = 5.0f;
    private const float ERROR_MARGIN = 0.1f;

    private GameObject[] theStack;
    private GameObject fakeBlock;
    private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

    private int stackIndex;
    private int scoreCount = 0;
    private int combo = 0;
    private int countPieces = 0;
    public int downCount = 0;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;

    private bool isMovingOnX = true;
    private bool gameOver = false;
    private bool skipFistBlock = false;

    private Vector3 desiredPostition;
    private Vector3 lastTilePosition;

    public List<GameObject> theListPieces;

    private void Start ()
    {
        theStack = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            theStack[i] = transform.GetChild(i).gameObject;

        stackIndex = transform.childCount - 1;

        lastGameMesh = theStack[stackIndex];
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
                FakeBlockSpawn();
                scoreCount++;
            }
            else
                EndGame();
        }
        MoveTile();
        MoveFakeingBlock();

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

    private GameObject FakeBlockSpawn ()
    {
        fakeBlock = Instantiate(lastGameMesh, theStack[stackIndex].transform.position, Quaternion.identity);
        //fakeBlock.transform.SetParent(theStack[stackIndex].transform);

        if (listFakeBlocks.Count >= 1)
        {
            Destroy(listFakeBlocks[0].gameObject);
            listFakeBlocks.Clear();
            listFakeBlocks.Add(fakeBlock);
        } else
            listFakeBlocks.Add(fakeBlock);
        return fakeBlock;
    }

    private void MoveFakeingBlock ()
    {
        if (fakeBlock)
            if (!isMovingOnX)
                fakeBlock.transform.position = new Vector3(
                    lastGameMesh.transform.position.x,
                    theStack[stackIndex].transform.position.y,
                    theStack[stackIndex].transform.position.z);
            else
                fakeBlock.transform.position = new Vector3(
                    theStack[stackIndex].transform.position.x,
                    theStack[stackIndex].transform.position.y,
                    lastGameMesh.transform.position.z);
    }

    private void MovigAtTheTile(GameObject obj, Transform tile)
    {
        obj.transform.position = tile.position;
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

        // Adding Material
        Material[] mats = new Material[lastMaterialObject.Count];
        for (int i = 0; i < lastMaterialObject.Count; i++)
            mats[i] = cupM;
        theStack[stackIndex].GetComponent<MeshRenderer>().materials = mats;
        // Adding Material
        
        // Change scale 
        cutObjectT.transform.localScale = theStack[stackIndex].transform.localScale;

        // 
        ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);
    }

    private bool PlaceTile ()
    {
        Transform t = theStack[stackIndex].transform;
        target.SetParent(theStack[stackIndex].transform);
        target.position = lastTilePosition;

        if (isMovingOnX) // X
        {
            float deltaX = lastTilePosition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {

                combo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);
                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;

                // CUT
                GameObject stack = Instantiate(theStack[stackIndex], t.transform.position, Quaternion.identity);

                float dist0 = theStack[stackIndex].transform.position.x * tileTransition;

                if (t.localPosition.x >= target.position.x)
                {
                    GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(
                        stack
                        , new Vector3(
                            cutObjectT.GetChild(0).position.x,
                            Mathf.Floor(theStack[stackIndex].transform.position.y),
                            cutObjectT.GetChild(0).position.z)
                        , transform.right
                        , cupM);


                    //Debug.Log(t.localPosition.x);
                    //Debug.Log("1");

                    if (!pieces[1].GetComponent<Rigidbody>())
                        pieces[1].AddComponent<Rigidbody>();

                    theListPieces.Add(pieces[0]);

                    // MATERIAL
                    lastMaterialObject.Clear();
                    for (int i = 0; i < pieces[0].GetComponent<MeshRenderer>().materials.Length; i++)
                        lastMaterialObject.Add(pieces[0].GetComponent<MeshRenderer>().materials[i]);

                    theStack[stackIndex].GetComponent<MeshRenderer>().enabled = false;

                    lastGameMesh = pieces[0]; // NEXT MESH
                    
                    Destroy(pieces[1], 1);
                } else
                {
                    GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(
                        stack
                        , new Vector3(
                            cutObjectT.GetChild(1).position.x,
                            Mathf.Floor(theStack[stackIndex].transform.position.y),
                            cutObjectT.GetChild(1).position.z)
                        , -transform.right
                        , cupM);

                    Debug.Log(t.position.x);
                    Debug.Log("2");

                    if (!pieces[1].GetComponent<Rigidbody>())
                        pieces[1].AddComponent<Rigidbody>();

                    theListPieces.Add(pieces[0]);

                    // MATERIAL
                    lastMaterialObject.Clear();
                    for (int i = 0; i < pieces[0].GetComponent<MeshRenderer>().materials.Length; i++)
                        lastMaterialObject.Add(pieces[0].GetComponent<MeshRenderer>().materials[i]);
                    
                    theStack[stackIndex].GetComponent<MeshRenderer>().enabled = false;
                    
                    lastGameMesh = pieces[0];  // NEXT MESH  

                    Destroy(pieces[1], 1);
                }

                // CUT

                // Scale

                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                // Scale
             }
        }
        else // Z
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

                GameObject stack = Instantiate(theStack[stackIndex], t.position, t.rotation);

                float dist0 = theStack[stackIndex].transform.position.z * tileTransition;

                if (t.localPosition.z <= target.position.z)
                {
                    GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(
                        stack
                        , new Vector3(
                            cutObjectT.GetChild(2).position.x,
                            Mathf.Floor(theStack[stackIndex].transform.position.y),
                            cutObjectT.GetChild(2).position.z)
                        , -transform.forward
                        , cupM);

                    Debug.Log(t.position.z);
                    Debug.Log("3");

                    if (!pieces[1].GetComponent<Rigidbody>())
                        pieces[1].AddComponent<Rigidbody>();

                    theListPieces.Add(pieces[0]);
                    // MATERIAL
                    lastMaterialObject.Clear();
                    for (int i = 0; i < pieces[0].GetComponent<MeshRenderer>().materials.Length; i++)
                        lastMaterialObject.Add(pieces[0].GetComponent<MeshRenderer>().materials[i]);

                    theStack[stackIndex].GetComponent<MeshRenderer>().enabled = false;

                    lastGameMesh = pieces[0];  // NEXT MESH

                    Destroy(pieces[1], 1);
                }
                else
                {
                    GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(
                        stack
                        , new Vector3(
                            cutObjectT.GetChild(3).position.x,
                            Mathf.Floor(theStack[stackIndex].transform.position.y),
                            cutObjectT.GetChild(3).position.z)
                        , transform.forward
                        , cupM);

                    Debug.Log(t.position.z);
                    Debug.Log("4");

                    if (!pieces[1].GetComponent<Rigidbody>())
                        pieces[1].AddComponent<Rigidbody>();

                    theListPieces.Add(pieces[0]);

                    // MATERIAL
                    lastMaterialObject.Clear();
                    for (int i = 0; i < pieces[0].GetComponent<MeshRenderer>().materials.Length; i++)
                        lastMaterialObject.Add(pieces[0].GetComponent<MeshRenderer>().materials[i]);

                    theStack[stackIndex].GetComponent<MeshRenderer>().enabled = false;

                    lastGameMesh = pieces[0];  // NEXT MESH

                    Destroy(pieces[1], 1);
                }

                // Scale
                t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
                // Scale
            }

        }

        // MOVE PIECE

        if (skipFistBlock)
            for (int i = 0; i < theListPieces.Count; i++)
                theListPieces[i].transform.position = new Vector3
                    (
                        theListPieces[i].transform.position.x,
                        Mathf.Floor(theListPieces[i].transform.position.y - 1f),
                        theListPieces[i].transform.position.z
                    );

        skipFistBlock = true;

        cutObjectT.position = t.position;

        // Change direction
        secondaryPosition = (isMovingOnX)
            ? t.localPosition.x
            : t.localPosition.z;
        isMovingOnX = !isMovingOnX;

        return true;
    }

    private bool MeshDisable(GameObject obj)
    {
        Destroy(obj.GetComponent<MeshRenderer>());
        obj.GetComponent<MeshRenderer>().enabled = false;
        if (!obj.activeSelf)
            return true;

         return false;
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
