using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DIRECTIONS { None, Right, Left, Down, Up }

public enum RoomType { Init, Middle, Normal, Exit, End, Treasure, Boss, Branch, Statue }

public class StructuralLevel : MonoBehaviour
{
    public bool _createSeed;
    public int _currentSeed;

    public int LEVEL_SIZE_X;
    public int LEVEL_SIZE_Y;

    public int MAIN_PATH_LENGTH;

    public float CREATE_BRANCH_PROBABILITY;
    public int BRANCH_MAX_LENGTH;

    public GameObject InitRoom;
    public GameObject room1x1;
    public GameObject room2x1;
    public GameObject room3x1;
    public GameObject room1x2;
    public GameObject room2x2;
    public GameObject room3x2;

    public StructuralRoom[,] rooms;
    private StructuralRoom firstRoom;

    //public int RoomTreasurePlace;
    //public int RoomStatuePlace;

    public void Generate()
    {
        if (_createSeed)
        {
            //Creates a seed every Generate() called
            _currentSeed = Random.Range(0, 10000);
        }
        Random.InitState(_currentSeed);
        //Size of the world
        rooms = new StructuralRoom[LEVEL_SIZE_X, LEVEL_SIZE_Y];

        int randomX = 0;
        int randomY = (int)(LEVEL_SIZE_Y / 2);

        ////Index for the room than will connect with treasure room
        //RoomTreasurePlace = Random.Range(1, MAIN_LINE_FIRST_HALF_LENGTH + 1);
        ////Index for the room than will connect with statue room
        //RoomStatuePlace = Random.Range(1, MAIN_LINE_SECOND_HALF_LENGTH + 1) + MAIN_LINE_FIRST_HALF_LENGTH + 1;

        //Reference for first room
        firstRoom = AddRoom(RoomType.Init, randomX, randomY, DIRECTIONS.None, 0, 0);

        //Generation using recursion
        firstRoom.GeneratePath();

        VisualizeLevel();
    }

    void VisualizeLevel()
    {
        for (int x = 0; x < LEVEL_SIZE_X; x++)
        {
            for (int y = 0; y < LEVEL_SIZE_Y; y++)
            {
                if (rooms[x, y] != null)
                {
                    if (rooms[x, y].type == RoomType.Init)
                    {
                        Instantiate(InitRoom, new Vector3(x, y), Quaternion.identity, transform);
                        continue;
                    }
                    int _sizeX = rooms[x, y].sizeX;
                    int _sizeY = rooms[x, y].sizeY;
                    if (_sizeX == 1)
                    {
                        if (_sizeY == 1)
                        {
                            Instantiate(room1x1, new Vector3(x, y), Quaternion.identity, transform);
                        }
                        else if (_sizeY == 2)
                        {
                            Instantiate(room1x2, new Vector3(x, y), Quaternion.identity, transform);
                        }
                    }
                    else if (_sizeX == 2)
                    {
                        if (_sizeY == 1)
                        {
                            Instantiate(room2x1, new Vector3(x, y), Quaternion.identity, transform);
                        }
                        else if (_sizeY == 2)
                        {
                            Instantiate(room2x2, new Vector3(x, y), Quaternion.identity, transform);
                        }
                    }
                    else if (_sizeX == 3)
                    {
                        if (_sizeY == 1)
                        {
                            Instantiate(room3x1, new Vector3(x, y), Quaternion.identity, transform);
                        }
                        else if (_sizeY == 2)
                        {
                            Instantiate(room3x2, new Vector3(x, y), Quaternion.identity, transform);
                        }
                    }
                }
            }
        }
    }

    //Make a room base on coordinate (_xBase,_yBase)
    public StructuralRoom AddRoom(RoomType type, int _xBase, int _yBase, DIRECTIONS firstExitDirection, int originDistance, int branchDistance)
    {
        return new StructuralRoom(this, type, _xBase, _yBase, firstExitDirection, originDistance, branchDistance);
    }
}


public class StructuralRoom 
{
    public int baseX, baseY, distanceToFirstRoom, distanceToBranchRoom;
    public int sizeX, sizeY;

    DIRECTIONS horizontalDirection, verticalDirection;

    public RoomType type;

    StructuralLevel structuralLevel;

    private const int Init_x_size = 2;
    private const int Init_y_size = 1;

    const int MAX_SIZE_X = 3;
    const int MAX_SIZE_Y = 2;

    List<RoomExitTree> exits = new List<RoomExitTree>();

    public StructuralRoom(StructuralLevel levelTree, RoomType type, int _xBase, int _yBase, DIRECTIONS enterDirection, int originDistance, int branchDistance)
    {
        baseX = _xBase;
        baseY = _yBase;

        this.type = type;
        distanceToFirstRoom = originDistance;
        distanceToBranchRoom = branchDistance;

        this.structuralLevel = levelTree;
        exits = new List<RoomExitTree>();
        //AddEnterExit(_xBase, _yBase, enterDirection);

        int currentX = _xBase;
        int currentY = _yBase;
        switch (type)
        {
            case RoomType.Init:
                BuildInitRoom(currentX, currentY);
                break;
            case RoomType.Normal:
                BuildNormalRoom(currentX, currentY);
                AddNormalRoomExits(_xBase, _yBase);
                break;
                //case RoomType.Branch:
                //    BuildBranchRoom(currentX, currentY);
                //    AddBranchRoomExits(_xBase, _yBase);
                //    break;
        }
    }

    #region Useful functions
    private const int MAX_ROOM_BUILD_DIRECTION_TRIES = 3;
    public DIRECTIONS GetValidRoomBuildDirection(int num_tries, int x, int y, int minRangeDirection = 0, int maxRangeDirection = 4)
    {
        //if num_tries > MAX_ROOM_BUILD_DIRECTION_TRIES the method ends and returns Directions.None
        if (num_tries > MAX_ROOM_BUILD_DIRECTION_TRIES) return DIRECTIONS.None;

        //First select a random direction to check
        DIRECTIONS direction = (DIRECTIONS)Random.Range(minRangeDirection, maxRangeDirection + 1);
        switch (direction)
        {
            case DIRECTIONS.Left:
                //if x==0 or left is ocuped, try again adding one to count
                if (x == 0 || GetLeft(x, y) != null)
                    return GetValidRoomBuildDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
            case DIRECTIONS.Right:
                //if x>=structurallevel.levelsizex - 1 or right is ocuped, try again adding one to count
                if (x >= structuralLevel.LEVEL_SIZE_X - 1 || GetRight(x, y) != null)
                    return GetValidRoomBuildDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
            case DIRECTIONS.Up:
                //if x>=structurallevel.levelsizey - 1 or up is ocuped, try again adding one to count
                if (y >= structuralLevel.LEVEL_SIZE_Y - 1 || GetUp(x, y) != null)
                    return GetValidRoomBuildDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
            case DIRECTIONS.Down:
                //if y==0 or down is ocuped, try again adding one to count
                if (y == 0 || GetDown(x, y) != null)
                    return GetValidRoomBuildDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
        }
        //if a conflict wasnt found, returns direction.
        return direction;
    }

    public int GetValidSpaces(DIRECTIONS direction, int x, int y, int MAX_SIZE, int num_tries)
    {
        int MAX_SPACE_TRIES = MAX_SIZE;
        if (num_tries > MAX_SPACE_TRIES)
        {
            return 0;
        }
        int result = 0;
        switch (direction)
        {
            case DIRECTIONS.Left:
                if (x == 0)
                {
                    return 0;
                }
                if (GetLeft(x, y) == null)
                {
                    result = GetValidSpaces(direction, x - 1, y, MAX_SIZE, num_tries + 1) + 1;
                }
                return result;
            case DIRECTIONS.Right:
                if (x >= structuralLevel.LEVEL_SIZE_X - 1)
                {
                    return 0;
                }
                if (GetRight(x, y) == null)
                {
                    result = GetValidSpaces(direction, x + 1, y, MAX_SIZE, num_tries + 1) + 1;
                }
                return result;
            case DIRECTIONS.Up:
                if (x >= structuralLevel.LEVEL_SIZE_Y - 1)
                {
                    return 0;
                }
                if (GetUp(x, y) == null)
                {
                    result = GetValidSpaces(direction, x, y + 1, MAX_SIZE, num_tries + 1) + 1;
                }
                return result;
            case DIRECTIONS.Down:
                if (y == 0)
                {
                    return 0;
                }
                if (GetDown(x, y) == null)
                {
                    result = GetValidSpaces(direction, x, y - 1, MAX_SIZE, num_tries + 1) + 1;
                }
                return result;
            default:
            case DIRECTIONS.None:
                return 0;
        }
    }

    #region Get room from a direction
    public StructuralRoom GetRight(int x, int y)
    {
        x++;
        if (x >= structuralLevel.LEVEL_SIZE_X)
            return null;
        return structuralLevel.rooms[x, y];
    }

    public StructuralRoom GetUp(int x, int y)
    {
        y++;
        if (y >= structuralLevel.LEVEL_SIZE_Y)
            return null;
        return structuralLevel.rooms[x, y];
    }

    public StructuralRoom GetLeft(int x, int y)
    {
        x--;
        if (x < 0)
            return null;
        return structuralLevel.rooms[x, y];
    }

    public StructuralRoom GetDown(int x, int y)
    {
        y--;
        if (y < 0)
            return null;
        return structuralLevel.rooms[x, y];
    }
    #endregion
    #endregion

    private void BuildInitRoom(int currentX, int currentY)
    {
        //Size x and y of the room, allways is 2x1.
        sizeX = Init_x_size;
        sizeY = Init_y_size;

        horizontalDirection = DIRECTIONS.Right;
        verticalDirection = DIRECTIONS.None;

        //set space as ocuped
        structuralLevel.rooms[currentX, currentY] = this;
        currentX++;
        //set space as ocuped
        structuralLevel.rooms[currentX, currentY] = this;

        //creates an exit at right. Next room will be a normal type room.
        exits.Add(AddExit(DIRECTIONS.Right, currentX, currentY, RoomType.Normal));
    }

    /// <summary>
	/// Builds the normal room.
	/// </summary>
	/// <param name="currentX">Base x position.</param>
	/// <param name="currentY">Base y position</param>
	private void BuildNormalRoom(int currentX, int currentY)
    {
        //_xReason and _yReason will be used for iteration in a future
        int _xReason = 0;
        int _yReason = 0;
        //mainDirection and secondaryDirection define how the room will be expanded
        DIRECTIONS mainDirection = DIRECTIONS.None;
        DIRECTIONS secondaryDirection = DIRECTIONS.None;

        //Lenght for the room based on directions
        int mainLength;
        int secondaryLength;
        int[] secondaryDistances;

        //Select the main direction to expand the room
        mainDirection = GetValidRoomBuildDirection(1, currentX, currentY, (int)DIRECTIONS.Right, (int)DIRECTIONS.Up);
        //Gets the max possible distance for the main direction
        mainLength = GetValidSpaces(mainDirection, currentX, currentY, (getXReason(mainDirection) * MAX_SIZE_X + getYReason(mainDirection) * MAX_SIZE_Y), 1);

        //Select a random distance for the main direction, based on max lenght possible
        mainLength = Random.Range(1, mainLength + 1);

        if (mainDirection != DIRECTIONS.None)
        {
            //Select the secondary direction based on main direction. 
            //if main direction is right or left, secondary direction is up or down;
            //if secondary direction is up or down, secondary direction is right or left
            secondaryDirection = (mainDirection == DIRECTIONS.Left || mainDirection == DIRECTIONS.Right) ?
                GetValidRoomBuildDirection(1, currentX, currentY, (int)DIRECTIONS.Down, (int)DIRECTIONS.Up) :
                GetValidRoomBuildDirection(1, currentX, currentY, (int)DIRECTIONS.Right, (int)DIRECTIONS.Left);
        }
        //Array than stores the possible secondary distances, and is used to calculate the max secondary distance of the room
        secondaryDistances = new int[mainLength];

        switch (mainDirection)
        {
            case DIRECTIONS.Left:
            case DIRECTIONS.Right:
                //if mainDirection is right, _xReason is 1, if is left, _xReason is -1
                _xReason = (mainDirection == DIRECTIONS.Right) ? 1 : (-1);
                if (secondaryDirection != DIRECTIONS.None)
                    _yReason = (secondaryDirection == DIRECTIONS.Up) ? 1 : (-1);

                //Iterates every space used in the main direction, gets secondary valid spaces for each one. 
                for (int i = 0; i < mainLength; i++)
                {
                    secondaryDistances[i] = GetValidSpaces(
                        secondaryDirection, currentX + i * _xReason, currentY,
                        (getXReason(secondaryDirection) * MAX_SIZE_X + getYReason(secondaryDirection) * MAX_SIZE_Y), 1);

                    if (secondaryDistances[i] > MAX_SIZE_Y)
                    {
                        secondaryDistances[i] = MAX_SIZE_Y;
                    }
                }
                //Gets the lowest distance for each secondary distance
                secondaryLength = Mathf.Min(secondaryDistances);
                //select a random value for secondary distance
                secondaryLength = Random.Range(1, secondaryLength + 1);

                //Sets spaces of the room as ocuped
                for (int i = 0; i < mainLength; i++)
                {
                    for (int j = 0; j < secondaryLength; j++)
                    {
                        structuralLevel.rooms[currentX + i * _xReason, currentY + j * _yReason] = this;
                    }
                }

                //Sets size of the room
                sizeX = mainLength;
                sizeY = secondaryLength;

                //Sets direction of the room
                horizontalDirection = mainDirection;
                verticalDirection = secondaryDirection;
                break;
            case DIRECTIONS.Down:
            case DIRECTIONS.Up:
                //if mainDirection is up, _yReason is 1, if is left, _yReason is -1
                _yReason = (mainDirection == DIRECTIONS.Up) ? 1 : (-1);
                if (secondaryDirection != DIRECTIONS.None)
                    _xReason = (secondaryDirection == DIRECTIONS.Right) ? 1 : (-1);

                //Iterates every space used in the main directions, gets secondary valid spaces for each one
                for (int i = 0; i < mainLength; i++)
                {
                    secondaryDistances[i] = GetValidSpaces(secondaryDirection, currentX, currentY + i * _yReason, (getXReason(secondaryDirection) * MAX_SIZE_X + getYReason(secondaryDirection) * MAX_SIZE_Y), 1);
                    if (secondaryDistances[i] > MAX_SIZE_X)
                    {
                        secondaryDistances[i] = MAX_SIZE_X;
                    }
                }
                secondaryLength = Mathf.Min(secondaryDistances);
                secondaryLength = Random.Range(1, secondaryLength + 1);

                for (int i = 0; i < mainLength; i++)
                {
                    for (int j = 0; j < secondaryLength; j++)
                    {
                        structuralLevel.rooms[currentX + j * _xReason, currentY + i * _yReason] = this;
                    }
                }
                sizeX = secondaryLength;
                sizeY = mainLength;

                horizontalDirection = secondaryDirection;
                verticalDirection = mainDirection;
                break;
            case DIRECTIONS.None:
            default:
                structuralLevel.rooms[currentX, currentY] = this;
                sizeX = 1;
                sizeY = 1;

                horizontalDirection = DIRECTIONS.None;
                verticalDirection = DIRECTIONS.None;
                break;
        }
    }

    void AddNormalRoomExits(int _xBase, int _yBase)
    {
        int exitXCoord;
        int exitYCoord;

        DIRECTIONS exitDirection = DIRECTIONS.None;
        RoomExitTree exit;
        RoomType roomType = RoomType.Normal;

        #region main_exit_creation
        exitXCoord = _xBase;
        exitYCoord = _yBase;

        //Room 1x1
        if (sizeX == 1 && sizeY == 1)
        {
            exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Right, DIRECTIONS.Up);
        }
        //Room 2x1
        if (sizeX == 2 && sizeY == 1)
        {
            exitXCoord += (sizeX - 1) * getXReason(horizontalDirection);
            exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Right, DIRECTIONS.Up);
        }
        //Room 1x2
        //Exits cant be placed at room base coords
        if (sizeX == 1 && sizeY == 2)
        {
            exitYCoord += (sizeY - 1) * getYReason(verticalDirection);
            exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Right, DIRECTIONS.Up);
        }
        //Room 2x2
        //Exits are placed in the other side than the base
        if (sizeX == 2 && sizeY == 2)
        {
            exitXCoord += (sizeX - 1) * getXReason(horizontalDirection);
            exitYCoord += (sizeY - 1) * getYReason(verticalDirection);
            exitDirection = DIRECTIONS.Right;
        }
        //Room 3x1
        //First exit are always placed at right
        if (sizeX == 3 && sizeY == 1)
        {
            exitXCoord += (sizeX - 1) * getXReason(horizontalDirection);
            exitDirection = DIRECTIONS.Right;
        }
        //First add the exit of the main path.
        exit = AddExit(exitDirection, exitXCoord, exitYCoord, roomType);
        exits.Add(exit);
        #endregion

        //branch exits are for alternative path
        #region branch_creation
        if ((structuralLevel.CREATE_BRANCH_PROBABILITY > Random.Range(0.0f, 1.0f)) && distanceToBranchRoom == 0)
        {
            roomType = RoomType.Branch;

            //Room 1x1
            if (sizeX == 1 && sizeY == 1)
            {
                exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Left, DIRECTIONS.Up);
            }
            //Room 2x1
            if (sizeX == 2 && sizeY == 1)
            {
                exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Left, DIRECTIONS.Up);
            }
            //Room 1x2
            if (sizeX == 1 && sizeY == 2)
            {
                exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Left, DIRECTIONS.Up);
            }
            //Room 2x2
            if (sizeX == 2 && sizeY == 2)
            {
                exitXCoord = _xBase;
                exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Down, DIRECTIONS.Up);
            }
            //Room 3x1
            if (sizeX == 3 && sizeY == 1)
            {
                exitXCoord -= getXReason(horizontalDirection);
                exitDirection = GetValidRoomExitDirection(1, exitXCoord, exitYCoord, DIRECTIONS.Down, DIRECTIONS.Up);
            }
            exit = AddExit(exitDirection, exitXCoord, exitYCoord, roomType);
            exits.Add(exit);
        }
        #endregion
    }

    void BuildBranchRoom(int currentX, int currentY)
    {

    }

    void AddBranchRoomExits(int _xBase, int _yBase)
    {

    }

    public DIRECTIONS GetValidRoomExitDirection(int num_tries, int x, int y, DIRECTIONS minRangeDirection, DIRECTIONS maxRangeDirection)
    {
        //if num_tries > MAX_ROOM_BUILD_DIRECTION_TRIES the method ends and returns Directions.None
        if (num_tries > MAX_ROOM_BUILD_DIRECTION_TRIES) return DIRECTIONS.None;

        //First select a random direction to check
        DIRECTIONS direction = (DIRECTIONS)Random.Range((int)minRangeDirection, (int)maxRangeDirection + 1);
        switch (direction)
        {
            case DIRECTIONS.Left:
                //if x==0 or left is ocuped, try again adding one to count
                if (x == 0 || GetLeft(x, y) != null)
                    return GetValidRoomExitDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
            case DIRECTIONS.Right:
                //if x>=structurallevel.levelsizex - 1 or right is ocuped, try again adding one to count
                if (x >= structuralLevel.LEVEL_SIZE_X - 1 || GetRight(x, y) != null)
                    return GetValidRoomExitDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
            case DIRECTIONS.Up:
                //if x>=structurallevel.levelsizey - 1 or up is ocuped, try again adding one to count
                if (y >= structuralLevel.LEVEL_SIZE_Y - 1 || GetUp(x, y) != null)
                    return GetValidRoomExitDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
            case DIRECTIONS.Down:
                //if y==0 or down is ocuped, try again adding one to count
                if (y == 0 || GetDown(x, y) != null)
                    return GetValidRoomExitDirection(num_tries + 1, x, y, minRangeDirection, maxRangeDirection);
                break;
        }
        //if a conflict wasnt found, returns direction.
        return direction;
    }

    public RoomExitTree AddExit(DIRECTIONS _direction, int _x, int _y, RoomType _type)
    {
        return new RoomExitTree(_direction, _x, _y, _type);
    }

    public int getXReason(DIRECTIONS _dir)
    {
        int result = 0;
        switch (_dir)
        {
            case DIRECTIONS.Left:
                result = -1;
                break;
            case DIRECTIONS.Right:
                result = 1;
                break;
        }
        return result;
    }

    public int getYReason(DIRECTIONS _dir)
    {
        int result = 0;
        switch (_dir)
        {
            case DIRECTIONS.Down:
                result = -1;
                break;
            case DIRECTIONS.Up:
                result = 1;
                break;
        }
        return result;
    }

    void AddEnterExit(int _x, int _y, DIRECTIONS _enterDirection)
    {
        exits.Add(new RoomExitTree(_enterDirection, _x, _y, exits[exits.Count - 1].NextRoomType));
    }

    public void GeneratePath()
    {
        //StructuralRoom newRoom = null;
        RoomExitTree currentExit;
        //Iterates over all the exits
        for (int i = 0; i < exits.Count; i++)
        {
            currentExit = exits[i];
            //Check if the exit is empty
            if (currentExit.NextRoom == null)
            {
                //if room type is branch add one to branch_distance counter
                int branch_distance = distanceToBranchRoom;
                if (exits[i].NextRoomType == RoomType.Branch)
                {
                    branch_distance++;
                }

                //Creates a room with coords depending on exit direction, and exit coords.
                switch (currentExit.Direction)
                {
                    case DIRECTIONS.Left:
                        currentExit.NextRoom = structuralLevel.AddRoom(exits[i].NextRoomType, exits[i].x - 1, exits[i].y, exits[i].Direction, distanceToFirstRoom + 1, branch_distance);
                        break;
                    case DIRECTIONS.Right:
                        currentExit.NextRoom = structuralLevel.AddRoom(exits[i].NextRoomType, exits[i].x + 1, exits[i].y, exits[i].Direction, distanceToFirstRoom + 1, branch_distance);
                        break;
                    case DIRECTIONS.Up:
                        currentExit.NextRoom = structuralLevel.AddRoom(exits[i].NextRoomType, exits[i].x, exits[i].y + 1, exits[i].Direction, distanceToFirstRoom + 1, branch_distance);
                        break;
                    case DIRECTIONS.Down:
                        currentExit.NextRoom = structuralLevel.AddRoom(exits[i].NextRoomType, exits[i].x, exits[i].y - 1, exits[i].Direction, distanceToFirstRoom + 1, branch_distance);
                        break;
                }
                //update current exit
                exits[i] = currentExit;
                //if exit is not null, continue generating path
                if (exits[i].NextRoom != null)
                {
                    exits[i].NextRoom.GeneratePath();
                }
            }
        }
    }
}

public class RoomExitTree
{
    public int x, y;

    public RoomType NextRoomType;

    public DIRECTIONS Direction;

    public StructuralRoom NextRoom;

    public RoomExitTree(DIRECTIONS _dir, int _x, int _y, RoomType _nextRoomType)
    {
        Direction = _dir;
        x = _x;
        y = _y;
        NextRoomType = _nextRoomType;
    }
}
