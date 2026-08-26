using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using ProceduralHelperGen;
using LogicSolver;

public class ProceduralRoomGen : MonoBehaviour
{
    
    // Singleton -------------------------------
    private static ProceduralRoomGen _instance;

    public static ProceduralRoomGen Instance { get { return _instance; } }
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
    }
    // Singleton 

    [Header("References")]
    [SerializeField] private GameObject doorFullPrefab;
    [SerializeField] private List<ObjectDataTemplate> objectDataTemplates;
    [SerializeField] private DifficultyTemplate difficultySettings;

    [Header("Parameters")]
    [SerializeField] private float roomDistance = 20f;
    [SerializeField] private float objectDoorDistance = 5f; // how much in front of the doors
    [SerializeField] private float objectToObjectDistance = 0; // handles offsets from object to object
    [SerializeField] private float objectInvalidationDistance = 0; // handles offsets for object invalidation

    //[SerializeField] private float doorDistance = 15; // accounts for doorframes
    [SerializeField] private float sideWallsDistance = 15f;
    [SerializeField] private float objectRatio = 0.5f;


    // Initialized variables
    private System.Random proceduralRandGen;
    private float doorSize;
    private int objectCount = 0;
    private int seed = 0;
    
    // Constants
    
    

    // Saves current doors in the room
    private List<DoorData> currentDoors = new List<DoorData>();

    private struct DoorData
    {
        public GameObject fullDoor;
        public Door doorComponent;

        public DoorData(GameObject givenDoor)
        {
            fullDoor = givenDoor;
            doorComponent = givenDoor.transform.GetChild(1).GetComponent<Door>();
        }
        public DoorData(GameObject givenDoor, Door givenDoorChild)
        {
            fullDoor = givenDoor;
            doorComponent = givenDoorChild;
        }
    }

    // StartGenerationProcess
    public void GenerateProcess()
    {
        Initialize();

        // Generate room 1, always with two doors
        GenerateNextRoom(transform.position, 2, 1);
        
        RoomSolution roomSolution = Solver.Solve(GameManager.Instance.worldState.roomStates[0].roomSettings);
        Debug.Log($"Correct Door Room 1: {roomSolution.safeDoor}");
        AssignDoorData(roomSolution);

        // Generate rest of the rooms. <= cause rooms are one indexed
        for (int room = 2; room <= difficultySettings.roomCount; room++)
        {
            int safeDoorIndex = roomSolution.safeDoor;
            //Debug.Log($"Random int: {safeDoorIndex}");
            Debug.Log($"Current Doors: {currentDoors.Count}");
            Vector3 nextDoorPosition = currentDoors[safeDoorIndex].fullDoor.transform.position;
            nextDoorPosition.z += roomDistance;

            int numDoorsForThisRoom = Mathf.FloorToInt((room - 1) / difficultySettings.roomsPerDoorIncrease) + difficultySettings.minDoors;
            if (difficultySettings.maxDoors > 0)
            {
                numDoorsForThisRoom = Mathf.Min(numDoorsForThisRoom, difficultySettings.maxDoors);
            }
            GenerateNextRoom(nextDoorPosition, numDoorsForThisRoom, room);
            roomSolution = Solver.Solve(GameManager.Instance.worldState.roomStates[room-1].roomSettings);
            Debug.Log($"Correct Door Room {room + 1}: {roomSolution.safeDoor}");
            AssignDoorData(roomSolution);
        }
    
        // Clear all doors
        currentDoors.Clear();
    }

    private void Initialize()
    {
        SpriteRenderer doorSpriteRenderer = doorFullPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>();
        doorSize = doorSpriteRenderer.sprite.bounds.size.x * Mathf.Abs(doorSpriteRenderer.transform.localScale.x);
        
        seed = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
        Debug.Log($"Generated Seed: {seed}");

        proceduralRandGen = new System.Random(seed);

        objectCount = Mathf.Min(difficultySettings.minObjects, objectDataTemplates.Count);
    }

    // 
    private void GenerateNextRoom(Vector3 centralPosition, int doorCount, int room)
    {
        Debug.Log($"New Room: {room + 1}");
        GenerateRoomState(doorCount, centralPosition);
        GenerateGridState(room, Mathf.CeilToInt(doorSize * doorCount), objectRatio);
        //PrintGrid(room);
        GenerateDoors(centralPosition, doorCount, room);

        GenerateObjects(doorCount, room);
    }

    private void GenerateRoomState(int doorCount, Vector3 centralPosition)
    {
        GameManager.Instance.worldState.roomStates.Add(new RoomState(seed, centralPosition, doorCount, difficultySettings.solverDifficulty));
    }

    private void GenerateDoors(Vector3 centralPosition, int doorCount, int room)
    {
        if (doorFullPrefab == null) throw new Exception("ProceduralRoomGen: DoorPrefab is null");
        // safely clear the currnet doors which are stored
        currentDoors.Clear();


        // Collect door size, offset initially, and width
        float offsetDoorSide = (doorCount % 2 == 0) ? doorSize / 2f : 0;
        float width = doorSize * doorCount;
        //Debug.Log($"Object Size (W): {doorSize}");
        //Debug.Log($"Door Count: {doorCount}");
        
        // Iterate through doors
        for (int currDoor = doorCount - 1; currDoor >= 0; currDoor--)
        {
            // Finds the position of the x coordinate of the new door, offsetting for initial off, and for door frames
            
            Vector3 newPosition = new Vector3(centralPosition.x + (((int)(currDoor - doorCount / 2) * (doorSize)) + offsetDoorSide), centralPosition.y, centralPosition.z);
            GameObject door = Instantiate(doorFullPrefab, newPosition, Quaternion.identity, transform);

            Door doorScript = door.transform.GetChild(1).GetComponent<Door>();
            if (doorScript == null) throw new Exception("ProceduralRoomGen: Door script not found on instantiated door");
            doorScript.SetDialogue("Missing Dialogue");
            doorScript.SetNumber(currDoor);

            // Invalidate the position of the door in the grid
            InvalidatePlacements(GameManager.Instance.worldState.roomStates[room-1].gridState, GetMaxWidthOfObject(door.transform.GetChild(1).gameObject), newPosition, doorCount, room);

            // Save the door
            currentDoors.Add(new DoorData(door, doorScript));
        }

        currentDoors.Reverse();
    }

    // AssignDoorData
    // Goes through each door and assigns proper given dialogue from the solver
    public void AssignDoorData(RoomSolution roomSolution)
    {
        currentDoors[roomSolution.safeDoor].doorComponent.SetIsSafe(true);
        for (int currDoor = 0; currDoor < currentDoors.Count; currDoor++)
        {
            currentDoors[currDoor].doorComponent.SetDialogue(roomSolution.statements[currDoor]);
        }
    }

    // GenerateObjects
    // Generates and initalized objects into the room based on all settings
    private void GenerateObjects(int doorCount, int room)
    {
        if (GameManager.Instance.worldState.roomStates.Count <= room-1 || GameManager.Instance.worldState.roomStates[room-1].gridState == null) throw new Exception($"GenerateObjects: roomState index exceeded {GameManager.Instance.worldState.roomStates.Count}, or ObjectsState is null {GameManager.Instance.worldState.roomStates[room-1].gridState}");
        // iterate the curr objects
        int currTotalObjects = Mathf.Min(difficultySettings.minObjects + Mathf.FloorToInt(room / difficultySettings.durationUntilObjectIncrease), Mathf.Min(difficultySettings.maxObjects, objectDataTemplates.Count)); 
        //if (room % difficultySettings.durationUntilObjectIncrease)

        List<int> indices = new List<int>();
        for (int i = 0; i < objectDataTemplates.Count; i++) indices.Add(i);

        Debug.Log($"Generating Objects: {currTotalObjects}");
        //PrintGrid(room);

        for(int currObject = 0; currObject < currTotalObjects; currObject++)
        {
            int randomIndex = proceduralRandGen.Next(0, indices.Count);
            //Debug.Log($"RandomIndex for Object: {randomIndex}");
            
            float width = GetMaxWidthOfObject(objectDataTemplates[randomIndex].GetObjectPrefab());

            GridState objectsState = GameManager.Instance.worldState.roomStates[room-1].gridState;
            Vector3 placementPosition = SetupRandomizedPlacement(objectsState, GameManager.Instance.worldState.roomStates[room-1].globalPosition, width, doorCount, room);
            if (placementPosition == new Vector3(0, 0, 0)) throw new Exception("GeneratedObjects: no valid positions for object");
            GameObject objectGenerated = Instantiate(objectDataTemplates[randomIndex].GetObjectPrefab(), placementPosition, Quaternion.identity, transform);
            ProceduralObjectGen.GenerateRandomForEachSprite(objectGenerated, objectDataTemplates[randomIndex].GetObjectPropertyDatas(), proceduralRandGen);

            // prevents duplicate prefabs
            indices.RemoveAt(randomIndex);
        }

        
        //PrintGrid(room);
    }
    
    // SetupRandomizedPlacement
    // Randomly finds a place and returns a random position within those grids, making sure to invalidate
    // Meant prior before object initalization.
    private Vector3 SetupRandomizedPlacement(GridState objectsState, Vector3 centerRoomPosition, float objectWidth, int doorCount, int room)
    {
        float roomWidth = doorSize * doorCount;

        // Generate a list of valid starting Grid IDs
        List<int> validIndices = ValidPlacements(objectsState, objectWidth);
        //Debug.Log($"Valid Counts: {validIndices}");
        if (validIndices.Count == 0) return new Vector3(0, 0, 0);

        // finds the valid grid ID from available options
        int randomIndex = proceduralRandGen.Next(0, validIndices.Count);
        int randomStartGridID = validIndices[randomIndex];

        // finds parition size the object takes up
        int placementObjectWidth = FindPlacementWidth(objectWidth);

        for (int x = 0; x < placementObjectWidth; x++) objectsState.availableGrids.Remove(randomStartGridID + x);

        //validIndices.Remove();
        
        float placementBetweenDistance = objectsState.ratio;
        
        Vector3 position = new Vector3(
            centerRoomPosition.x - (roomWidth / 2.0f) + placementBetweenDistance * randomStartGridID + objectWidth/2.0f, 
            centerRoomPosition.y, 
            centerRoomPosition.z - objectDoorDistance);
        return position;
    }

    // if someone wants to fix this feel free, it should be more generalized
    // InvalidatePlacements
    // Finds and sets valid indices to invalid with float size of the object, and position
    private void InvalidatePlacements(GridState objectsState, float width, Vector3 position, int doorCount, int room)
    {
        // relevant to grid
        int placementSize = FindPlacementWidth(width + objectInvalidationDistance);
        // roomWidth
        float roomWidth = doorSize * doorCount;
        // the edge position of the room in global coordinates
        float leftEdgeRoomPos = GameManager.Instance.worldState.roomStates[room-1].globalPosition.x - (roomWidth / 2.0f);
        // the far left index grid of the object, and how far it is
        int startGridID = Mathf.FloorToInt((position.x - (width / 2.0f) - leftEdgeRoomPos) / objectRatio);

        //Debug.Log($"start grid id: {startGridID}");
        for (int x = 0; x < placementSize ; x++) objectsState.availableGrids.Remove(x + startGridID);
        
    }

    // ValidPlacements
    // Returns valid list of potential indices in relation to the available grid of objects state
    private List<int> ValidPlacements(GridState objectsState, float width)
    {
        int placementSize = FindPlacementWidth(width + objectToObjectDistance);
        List<int> potentialIndices = new List<int>();
        //Debug.Log($"Object Size: {placementSize}");
        
        int prevIndex = 0;
        for (int x = 1; x < objectsState.availableGrids.Count; x++)
        {
            if (objectsState.availableGrids[x-1] + 1 != objectsState.availableGrids[x])
            {
                prevIndex = x;
            }
            
            // Figure out if the length if long enough to fit object
            int length = (x - prevIndex) + 1;
            if (length >= placementSize)
            {
                // finds actual grid ID, because it is not ordered
                potentialIndices.Add(objectsState.availableGrids[x - placementSize + 1]);
            }
        }

        return potentialIndices;
    }

    // FindPlacementWidth()
    // Finds how many indices the objects width might take up, rounded up
    private int FindPlacementWidth(float width)
    {
        return (int) Math.Ceiling(width / objectRatio);
    }

    // GenerateGridState()
    // Generates an GridState with given parameter values, and adds to the current GameState
    private void GenerateGridState(int room, int roomWidth, float ratio = 1.0f)
    {
        if (GameManager.Instance.worldState.roomStates.Count -1 >= room) throw new Exception("GenerateObjectsState: Exceeded roomStates size in worldState");
        GridState gridState = new GridState();
        gridState.ratio = ratio;
        gridState.InitializeValidPositions(roomWidth);
        GameManager.Instance.worldState.roomStates[room-1].gridState = gridState;
    }

    // GetMaxWidthOfObject()
    // Finds all sprites and finds the largest width of the sprite and returns it
    private float GetMaxWidthOfObject(GameObject givenObject)
    {
        SpriteRenderer[] spriteRenderers = givenObject.GetComponentsInChildren<SpriteRenderer>(true);
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



    // helper for printing the grid of what is available
    private void PrintGrid(int room)
    {
        GridState gridState = GameManager.Instance.worldState.roomStates[room-1].gridState;
        string output = "";
        for (int i = 0; i < gridState.availableGrids.Count; i++)
        {
            output += gridState.availableGrids[i] + " ";
        }
        Debug.Log(output);
    }
    
}
