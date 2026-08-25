using UnityEngine;
using System;
using System.Collections.Generic;
//using System.Numerics;

public class ProceduralRoomGen : MonoBehaviour
{
    // Singleton -------------------------------
    private static ProceduralRoomGen _instance;

    public static ProceduralRoomGen Instance { get { return _instance; } }

    System.Random proceduralRandGen;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple ProceduralRoomGen scripts. Bad!");
        }
        else
        {
            _instance = this;
        }


        doorSpriteRenderer = doorFullPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>();
        doorSize = doorSpriteRenderer.sprite.bounds.size.x * Mathf.Abs(doorSpriteRenderer.transform.localScale.x);
        
        seed = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
        Debug.Log($"Generated Seed: {seed}");


        proceduralRandGen = new System.Random(seed);
    }


    [Header("References")]
    [SerializeField] private GameObject doorFullPrefab;
    [SerializeField] private List<GameObject> objectPrefabs;
    [SerializeField] private DifficultyTemplate difficultySettings;

    [Header("Parameters")]
    [SerializeField] private float roomDistance = 20;
    [SerializeField] private float objectDoorDistance = 5; // how much in front of the doors
    [SerializeField] private float objectToObjectDistance = 5;
    //[SerializeField] private float doorDistance = 15; // accounts for doorframes
    [SerializeField] private float sideWallsDistance = 15;


    // Initialized variables
    private float doorSize;
    private SpriteRenderer doorSpriteRenderer;
    int seed = 0;

    // Constants
    

    // Saves current doors in the room
    private List<DoorData> currentDoors = new List<DoorData>();


    private struct DoorData
    {
        public GameObject door;
        public bool safe;

        public DoorData(GameObject givenDoor, bool isSafe)
        {
            door = givenDoor;
            safe = isSafe;
        }
    }


    /*private struct ObjectData
    {
        public GameObject memorableObject;
        public float maxWidth; 

        public ObjectData(GameObject givenObject, float width)
        {
            memorableObject = givenObject;
            maxWidth = width;
        }
    }*/

    // StartGenerationProcess
    public void GenerateProcess()
    {
        GenerateNextRoom(transform.position, 2);
        
        for (int room = 2; room < difficultySettings.roomCount; room++)
        {
            int randomInt = proceduralRandGen.Next(0, room);
            Debug.Log($"Random int: {randomInt}");
            Debug.Log($"Current Doors: {currentDoors.Count}");
            Vector3 nextDoorPosition = currentDoors[randomInt].door.transform.position;
            nextDoorPosition.z += roomDistance;
            GenerateNextRoom(nextDoorPosition, (room-1) + difficultySettings.minDoors);
        }

        // Clear all doors
        currentDoors.Clear();
    }

    // 
    private void GenerateNextRoom(Vector3 centralPosition, int doorCount)
    {
        GenerateRoomState(doorCount);

        GenerateDoors(centralPosition, doorCount);
    }

    private void GenerateRoomState(int doorCount)
    {
        GameManager.Instance.worldState.roomStates.Add(new RoomState(seed, doorCount));
    }




    private void GenerateDoors(Vector3 centralPosition, int doorCount)
    {
        if (doorFullPrefab == null) throw new Exception("ProceduralRoomGen: DoorPrefab is null");
        // safely clear the currnet doors which are stored
        currentDoors.Clear();

        

        // Collect door size, offset initially, and width

        float offsetDoorSide = (doorCount % 2 == 0) ? doorSize / 2f : 0;
        float width = doorSize * doorCount;
        Debug.Log($"Object Size (W): {doorSize}");
        Debug.Log($"Door Count: {doorCount}");
        
        // Iterate through doors
        for (int currDoor = 0; currDoor < doorCount; currDoor++)
        {
            // finds the position of the x coordinate of the new door, offsetting for initial off, and for door frames
            
            Vector3 newPosition = new Vector3(centralPosition.x + (((int)(currDoor - doorCount / 2) * (doorSize)) + offsetDoorSide), centralPosition.y, centralPosition.z);
            GameObject door = Instantiate(doorFullPrefab, newPosition, Quaternion.identity, transform);
            door.transform.GetChild(1).GetComponent<Door>().SetNumber(currDoor);

            // save the door
            currentDoors.Add(new DoorData(door, false));
        }
    }

    private void GenerateObjects(int room)
    {
        int currTotalObjects = Mathf.Min(difficultySettings.minObjects + (room * difficultySettings.durationUntilObjectIncrease), difficultySettings.maxObjects);
        List<int> indices = new List<int>();
        for (int i = 0; i < objectPrefabs.Count; i++) indices.Add(0);

        for(int currObject = 0; currObject <= currTotalObjects; currObject++)
        {
            int randomIndex = proceduralRandGen.Next(0, indices.Count);
            indices.RemoveAt(randomIndex);

            objectPrefabs[randomIndex] = 
            GameObject objectGenerated = Instantiate(objectPrefabs[randomIndex], , Quaternion.identity, transform);

        }

        
        
    }

    // SetupRandomizedPlacement
    // Randomly finds a place and returns a random position within those grids, making sure to invalidate
    // Meant prior before object initalization.
    private Vector3 SetupRandomizedPlacement(ObjectsState objectsState, Vector3 centerRoomPosition, float objectWidth, int room)
    {
        float roomWidth = doorSize * ((room-1) + difficultySettings.minDoors);

        List<int> validIndices = ValidPlacements(objectsState, objectWidth);
        int randomIndex = proceduralRandGen.Next(0, validIndices.Count);
        int placementObjectWidth = FindPlacementWidth(objectsState, objectWidth);

        for (int x = placementObjectWidth -1; x >= 0; x--) validIndices.RemoveAt(x);
        
        float placementBetweenDistance = placementObjectWidth * objectsState.ratio;
        
        Vector3 position = new Vector3(centerRoomPosition.x - (roomWidth / 2.0f) + placementBetweenDistance * randomIndex, centerRoomPosition.y, centerRoomPosition.z);
        return position;
    }

    // ValidPlacements
    // Returns valid list of potential indices in relation to the available grid of objects state
    private List<int> ValidPlacements(ObjectsState objectsState, float width)
    {
        //float doorWidth = GetSpriteLocalScaleX(doorFullPrefab.transform.GetChild(1).GetComponent<SpriteRenderer>());
        int placementSize = FindPlacementWidth(objectsState, width);
        List<int> potentialIndices = new List<int>();
        int prevIndex = 0;
        for (int x = 1; x < objectsState.availableGrids.Count; x++)
        {
            if (objectsState.availableGrids[x-1] + 1 != objectsState.availableGrids[x])
            {
                prevIndex = -1;
            }
            else if (prevIndex == -1)
            {
                prevIndex = x-1;
            }

            if (x - prevIndex > placementSize)
            {
                potentialIndices.Add(x - placementSize);
            }
        }

        return potentialIndices;
    }

    // FindPlacementWidth()
    // Finds how many indices the objects width might take up, rounded up
    private int FindPlacementWidth(ObjectsState objectsState, float width)
    {
        return (int) Math.Ceiling(objectsState.ratio * width);
    }

    // GenerateObjectsState()
    // Generates an ObjectsState with given parameter values, and adds to the current GameState
    private void GenerateObjectsState(int room, int roomWidth, float ratio = 1.0f)
    {
        if (GameManager.Instance.worldState.roomStates.Count -1 >= room) throw new Exception("GenerateObjectsState: Exceeded roomStates size in worldState");
        ObjectsState objectsState = new ObjectsState();
        objectsState.InitializeValidPositions(roomWidth);
        objectsState.ratio = ratio;
        GameManager.Instance.worldState.roomStates[room].objectsStates.Add(objectsState);
    }

    // GetMaxWidthOfObject()
    // Finds all sprites and finds the largest width of the sprite and returns it
    private float GetMaxWidthOfObject()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        float maxWidth = 0f;

        foreach(SpriteRenderer spriteRenderer in spriteRenderers)
        {
            float currWidth = GetSpriteLocalScaleX(spriteRenderer);
            if (currWidth > maxWidth) maxWidth = currWidth;
        }

        return maxWidth;
    }

    // GetSpriteLocalScaleX()
    // Grabs the local scale width of an object in the world in respect to the art asset
    private float GetSpriteLocalScaleX(SpriteRenderer spriteRenderer)
    {
        return spriteRenderer.sprite.bounds.size.x * Mathf.Abs(spriteRenderer.transform.localScale.x);
    }
}
