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
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject ceilingTiles;
    [SerializeField] private GameObject roomLightsPrefab;
    [SerializeField] private List<ObjectDataTemplate> objectDataTemplates;
    [SerializeField] private DifficultyTemplate difficultyIfNull;

    [Header("Parameters")]
    [SerializeField] private float roomDistance = 20f;
    [SerializeField] private float objectDoorDistance = 5f; // how much in front of the doors
    [SerializeField] private float objectGroundDistance = 10f; // how much in front of the doors
    [SerializeField] private float objectCeilingDistance = 10f; // how much in front of the doors
    [SerializeField] private List<float> objectRelHeights = new List<float> {0f, 0f, 5f}; // object heights (excluding ceiling since its tied to sprites)
    [SerializeField] private float objectToObjectDistance = 0; // handles offsets from object to object
    [SerializeField] private float objectInvalidationDoorDistance = 0; // handles offsets for object invalidation
    [SerializeField] private float roomLightsYPosition = 17f;


    //[SerializeField] private float doorDistance = 15; // accounts for doorframes
    //[SerializeField] private float sideWallsDistance = 15f;


    // Initialized variables
    private System.Random proceduralRandGen;
    private float doorFullSize;
    private float doorCompSize;
    private int objectCount = 0;
    private int seed = 0;
    private DifficultyTemplate difficulty;
    
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
        SetCeilingPosition();
        AddCeilingHeight();

        // Generate room 1, always with two doors
        GenerateNextRoom(transform.position, difficulty.minDoors, 0);
        
        RoomSolution roomSolution = DetailBuilder.SolveRoom(0, objectDataTemplates, difficulty.detailMentions);
        Debug.Log($"Correct Door Room 1: {roomSolution.safeDoor}");
        AssignDoorData(roomSolution);

        // Generate rest of the rooms. <= cause rooms are one indexed
        for (int room = 1; room < difficulty.roomCount; room++)
        {
            int safeDoorIndex = roomSolution.safeDoor;
            Debug.Log($"Random int: {safeDoorIndex}");
            Debug.Log($"Current Doors: {currentDoors.Count}");
            Vector3 nextDoorPosition = currentDoors[safeDoorIndex].fullDoor.transform.position;
            nextDoorPosition.z += roomDistance;

            // this might be wrong - chris
            int numDoorsForThisRoom = Mathf.FloorToInt((room) / difficulty.roomsPerDoorIncrease) + difficulty.minDoors;
            if (difficulty.maxDoors > 0)
            {
                numDoorsForThisRoom = Mathf.Min(numDoorsForThisRoom, difficulty.maxDoors);
            }
            Debug.Log($"Num doors : {numDoorsForThisRoom}");
            GenerateNextRoom(nextDoorPosition, numDoorsForThisRoom, room);

            roomSolution = DetailBuilder.SolveRoom(room, objectDataTemplates, difficulty.detailMentions);
            Debug.Log($"Correct Door Room {room + 1}: {roomSolution.safeDoor}");

            AssignDoorData(roomSolution);
        }
    
        // Clear all doors
        currentDoors.Clear();
    }

    private void Initialize()
    {
        SpriteRenderer doorFullSpriteRenderer = doorFullPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>();
        doorFullSize = doorFullSpriteRenderer.sprite.bounds.size.x * Mathf.Abs(doorFullSpriteRenderer.transform.localScale.x);
        
        SpriteRenderer doorSpriteRenderer = doorFullPrefab.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        doorCompSize = doorSpriteRenderer.sprite.bounds.size.x * Mathf.Abs(doorSpriteRenderer.transform.localScale.x);

        seed = GameManager.Instance.currentSeed;
        proceduralRandGen = new System.Random(seed);

        if (GameManager.Instance.currentDifficulty == null)
        {
            difficulty = difficultyIfNull;
        }
        else
        {
            difficulty = GameManager.Instance.currentDifficulty;
        }

        objectCount = Mathf.Min(difficulty.minObjects, objectDataTemplates.Count);
    }

    // 
    private void GenerateNextRoom(Vector3 centralPosition, int doorCount, int room)
    {
        Debug.Log($"New Room: {room + 1}");
        GenerateRoomState(doorCount, centralPosition);
        GenerateRoomSpace(room, GetRoomWidth(doorCount));
        //Mathf.FloorToInt(doorSize * doorCount)
        //PrintGrid(room);
        GenerateDoors(centralPosition, doorCount, room);
        GenerateWalls(centralPosition, doorCount, room);
        GenerateRoomLights(centralPosition);

        GenerateObjects(doorCount, room);
    }

    private void GenerateRoomState(int doorCount, Vector3 centralPosition)
    {
        // we generate random seed using our seed, for deterministic values, while having each room be different, since right now
        // if we have the same door size, we get the same seed each time
        GameManager.Instance.worldState.roomStates.Add(new RoomState(proceduralRandGen.Next(), centralPosition, GetRoomWidth(doorCount), doorCount, difficulty.solverDifficulty));
    }

    private void GenerateDoors(Vector3 centralPosition, int doorCount, int room)
    {
        if (doorFullPrefab == null) throw new Exception("ProceduralRoomGen: DoorPrefab is null");
        // safely clear the currnet doors which are stored
        currentDoors.Clear();


        // Collect door size, offset initially, and width
        float offsetDoorSide = (doorCount % 2 == 0) ? doorFullSize / 2f : 0;
        float width = doorFullSize * doorCount;
        //Debug.Log($"Object Size (W): {doorSize}");
        //Debug.Log($"Door Count: {doorCount}");
        
        // Iterate through doors
        for (int currDoor = doorCount - 1; currDoor >= 0; currDoor--)
        {
            // Finds the position of the x coordinate of the new door, offsetting for initial off, and for door frames
            
            Vector3 newPosition = new Vector3(centralPosition.x + (((int)(currDoor - doorCount / 2) * (doorFullSize)) + offsetDoorSide), centralPosition.y, centralPosition.z);
            GameObject door = Instantiate(doorFullPrefab, newPosition, Quaternion.identity, transform);

            Door doorScript = door.transform.GetChild(1).GetComponent<Door>();
            if (doorScript == null) throw new Exception("ProceduralRoomGen: Door script not found on instantiated door");
            doorScript.SetDialogue("Missing Dialogue");
            doorScript.SetNumber(currDoor);

            // Invalidate the position of the door in the grid
           List<RoomRow> roomRows = GameManager.Instance.worldState.roomStates[room].roomSpace.roomRows;
           RoomRow.AddObject(roomRows, new Range(newPosition.x - doorCompSize / 2.0f, newPosition.x + doorCompSize / 2.0f), new List<int>{1,2});

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

    private void GenerateWalls(Vector3 centralPosition, int doorCount, int room)
    {
        if (wallPrefab == null) throw new Exception("ProceduralRoomGen: WallPrefab is null");

        float roomWidth = GetRoomWidth(doorCount);
       
        Vector3 rightPosition = new Vector3(centralPosition.x + (roomWidth / 2), centralPosition.y, centralPosition.z - (roomDistance / 2));
        Vector3 leftPosition = new Vector3(centralPosition.x - (roomWidth / 2), centralPosition.y, centralPosition.z - (roomDistance / 2));
        
        Instantiate(wallPrefab, rightPosition, Quaternion.Euler(0, 90, 0), transform);
        Instantiate(wallPrefab, leftPosition, Quaternion.Euler(0, 270, 0), transform);
    }

    // SetCeilingPosition
    // Finds the height of the doorframe/wall, and places the ceiling at that height
    private void SetCeilingPosition()
    {
        ceilingTiles.transform.position = new Vector3(transform.position.x, GetSpriteLocalScaleY(doorFullPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>()), transform.position.z);
    }

    private void AddCeilingHeight()
    {
        objectRelHeights.Add(ceilingTiles.transform.position.y);
    }

    private void GenerateRoomLights(Vector3 centralPosition)
    {
        if (roomLightsPrefab == null) throw new Exception("ProceduralRoomGen: RoomLightsPrefab is null");
       
        Vector3 position = new Vector3(centralPosition.x, roomLightsYPosition, centralPosition.z - (roomDistance / 2));
        Instantiate(roomLightsPrefab, position, Quaternion.Euler(90, 0, 0), transform);
    }

    // GenerateObjects
    // Generates and initalized objects into the room based on all settings
    private void GenerateObjects(int doorCount, int room)
    {
        if (GameManager.Instance.worldState.roomStates.Count <= room || GameManager.Instance.worldState.roomStates[room].roomSpace == null) throw new Exception($"GenerateObjects: roomState index exceeded {GameManager.Instance.worldState.roomStates.Count}, or ObjectsState is null {GameManager.Instance.worldState.roomStates[room].roomSpace}");
        // iterate the curr objects
        int currTotalObjects = Mathf.Min(difficulty.minObjects + Mathf.FloorToInt(room / difficulty.durationUntilObjectIncrease), Mathf.Min(difficulty.maxObjects, objectDataTemplates.Count)); 

        List<int> indices = new List<int>();
        for (int i = 0; i < objectDataTemplates.Count; i++) indices.Add(i);

        Debug.Log($"Generating Objects: {currTotalObjects}");
        //PrintGrid(room);

        for(int currObject = 0; currObject < currTotalObjects; currObject++)
        {
            int randomIndex = proceduralRandGen.Next(0, indices.Count);
            //Debug.Log($"RandomIndex for Object: {randomIndex}");
            
            float width = GetMaxWidthOfObject(objectDataTemplates[randomIndex].GetObjectPrefab()) + objectDataTemplates[randomIndex].GetInvalidationRange();

            RoomSpace roomSpace = GameManager.Instance.worldState.roomStates[room].roomSpace;
            Vector3 placementPosition = SetupRandomizedPlacement(roomSpace, objectDataTemplates[randomIndex], GameManager.Instance.worldState.roomStates[room].globalPosition, width, GetRoomWidth(doorCount));
            if (placementPosition == new Vector3(0, 0, 0)) throw new Exception("GeneratedObjects: no valid positions for object");
            GameObject objectGenerated = Instantiate(objectDataTemplates[randomIndex].GetObjectPrefab(), placementPosition, Quaternion.identity, transform);
            GameManager.Instance.worldState.roomStates[room].objects.Add(objectGenerated);
            
            ProceduralObjectGen.GenerateRandomForEachSprite(objectGenerated, objectDataTemplates[randomIndex].GetObjectPropertyDatas(), proceduralRandGen);

            // prevents duplicate prefabs
            indices.RemoveAt(randomIndex);
        }

        
        //PrintGrid(room);
    }
    
    // SetupRandomizedPlacement
    // Randomly finds a place and returns a random position within those grids, making sure to invalidate
    // Meant prior before object initalization.
    private Vector3 SetupRandomizedPlacement(RoomSpace roomSpace, ObjectDataTemplate objectDataTemplate, Vector3 centerRoomPosition, float objectWidth, float roomWidth)
    {
        // Generate a list of valid starting Grid IDs
        List<Range> validRanges = RoomRow.GetSharedFreeSpace(roomSpace.roomRows, objectDataTemplate.GetRowsTaken());
        validRanges.RemoveAll(r => r.Length < objectWidth);
        Debug.Log($"Valid Counts: {validRanges.Count}");
        if (validRanges.Count == 0) return new Vector3(0, 0, 0);

        // finds the valid grid ID from available options
        int randomIndex = proceduralRandGen.Next(0, validRanges.Count);
        Range randomRange = validRanges[randomIndex];
        float start = randomRange.Start + (float)proceduralRandGen.NextDouble() * (randomRange.Length - objectWidth);
        randomRange = new Range(start, start + objectWidth);
        RoomRow.AddObject(roomSpace.roomRows, randomRange, objectDataTemplate.GetRowsTaken());

        // finds parition size the object takes up
        float placementHalfWidth = (randomRange.End - randomRange.Start) / 2.0f;
        float distancePlacement = GetObjectDistances()[RoomSpace.FindRoomRowPlacement(objectDataTemplate.GetRowSource(), objectRelHeights.Count)];

        
        Vector3 position = new Vector3(
            randomRange.Start + placementHalfWidth, 
            centerRoomPosition.y + objectRelHeights[objectDataTemplate.GetRowSource()], 
            centerRoomPosition.z - distancePlacement);
        return position;
    }

    /*
    // if someone wants to fix this feel free, it should be more generalized
    // InvalidatePlacements
    // Finds and sets valid indices to invalid with float size of the object, and position
    private void InvalidatePlacements(RoomSpace roomSpace, float width, Vector3 position, int doorCount, int room)
    {
        // relevant to grid
        int placementSize = FindPlacementWidth(width + objectInvalidationDistance);
        float roomWidth = GetRoomWidth(doorCount);
        // the edge position of the room in global coordinates
        float leftEdgeRoomPos = GameManager.Instance.worldState.roomStates[room].globalPosition.x - (roomWidth / 2.0f);
        // the far left index grid of the object, and how far it is
        int startGridID = Mathf.FloorToInt((position.x - (width / 2.0f) - leftEdgeRoomPos) / objectRatio);

        //Debug.Log($"start grid id: {startGridID}");
        for (int x = 0; x < placementSize ; x++) roomSpace.roomRows.Remove(x + startGridID);
        
    }*/

    /*
    // ValidPlacements
    // Returns valid list of potential indices in relation to the available grid of objects state
    private List<int> ValidPlacements(RoomSpace roomSpace, float width)
    {
        int placementSize = FindPlacementWidth(width + objectToObjectDistance);
        List<int> potentialIndices = new List<int>();
        //Debug.Log($"Object Size: {placementSize}");
        
        int prevIndex = 0;
        for (int x = 1; x < roomSpace.roomRows.Count; x++)
        {
            if (roomSpace.roomRows[x-1] + 1 != roomSpace.roomRows[x])
            {
                prevIndex = x;
            }
            
            // Figure out if the length if long enough to fit object
            int length = (x - prevIndex) + 1;
            if (length > placementSize)
            {
                // finds actual grid ID, because it is not ordered
                potentialIndices.Add(roomSpace.roomRows[x - placementSize + 1]);
            }
        }

        return potentialIndices;
    }
    */

    // GenerateRoomSpace()
    // Generates an RoomSpace with given parameter values, and adds to the current GameState
    private void GenerateRoomSpace(int room, float roomWidth)
    {
        // this grid right here might have an issue with the change in room - 1   - chris
        if (GameManager.Instance.worldState.roomStates.Count -1 > room) throw new Exception("GenerateObjectsState: Exceeded roomStates size in worldState");
        
        List<float> objectDistances = GetObjectDistances();
        Debug.Log($"GenerateRoomSpace: objectDistances and objectHeights: {objectDistances.Count}  {objectRelHeights.Count}");
        RoomSpace roomSpace = new RoomSpace(roomWidth, GameManager.Instance.worldState.roomStates[room].globalPosition, objectDistances, objectRelHeights);
        GameManager.Instance.worldState.roomStates[room].roomSpace = roomSpace;
    }

    private List<float> GetObjectDistances()
    {
        return new List<float> {objectGroundDistance, objectDoorDistance, objectCeilingDistance};
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

    // GetSpriteLocalScaleY()
    // Grabs the local scale height of an object in the world in respect to the art asset
    private float GetSpriteLocalScaleY(SpriteRenderer spriteRenderer)
    {
        return spriteRenderer.sprite.bounds.size.y * Mathf.Abs(spriteRenderer.transform.localScale.y);
    }

    private float GetRoomWidth(int doorCount)
    {
        return doorFullSize * doorCount;
    }



    // helper for printing the grid of what is available
    private void PrintGrid(int room)
    {
        RoomSpace roomSpace = GameManager.Instance.worldState.roomStates[room].roomSpace;
        string output = "";
        for (int i = 0; i < roomSpace.roomRows.Count; i++)
        {
            output += roomSpace.roomRows[i] + " ";
        }
        Debug.Log(output);
    }
    
}