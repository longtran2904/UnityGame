using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RoomType
{
    Normal,
    Shop,
    Hub,
    Boss,
    Starting,

    Count
}

public class GenConnection
{
    public GenRoom a;
    public GenRoom b;

    public GenConnection globalNext;

    public RectInt rect;

    public GenConnection(GenRoom a, GenRoom b)
    {
        this.a = a;
        this.b = b;
    }
}

public class GenRoomConnection
{
    public GenConnection connection;
    public GenRoomConnection next;

    //public Vector2Int facingDir;
}

public class GenRoomInfo
{
    // TODO: Fill this in later
    public string name;
    public Vector2Int size;
    public List<RectInt> doors;
    public GameObject prefab;

    public GenRoomInfo(int width, int height, int doorCount, string name = null, GameObject prefab = null)
    {
        size = new Vector2Int(width, height);
        doors = new List<RectInt>(doorCount);
        this.name = name;
        this.prefab = prefab;
    }

    public void AddSimpleDoors(int doorLength, int doorOffset)
    {
        Vector2Int[] corners = { Vector2Int.zero, new Vector2Int(size.x, 0), size, new Vector2Int(0, size.y) };

        bool singleHorizontal = size.x < (doorLength + doorOffset) * 2;
        bool singleVertical = size.y < (doorLength + doorOffset) * 2;

        for (int i = 0; i < 4; i++)
        {
            Vector2Int corner = corners[i];
            Vector2Int horizontalOffset = corner.x == 0 ? Vector2Int.right * doorOffset : Vector2Int.left * (doorOffset + doorLength);
            Vector2Int verticalOffset   = corner.y == 0 ? Vector2Int.up    * doorOffset : Vector2Int.down * (doorOffset + doorLength);
            if (corner.x != 0)
                verticalOffset += Vector2Int.left;
            if (corner.y != 0)
                horizontalOffset += Vector2Int.down;

            if ((singleHorizontal && i % 2 == 0) || !singleHorizontal)
                doors.Add(new RectInt(corner + horizontalOffset, Vector2Int.right * (doorLength - 1) + Vector2Int.one));
            if ((singleVertical && i < 2) || !singleVertical)
                doors.Add(new RectInt(corner + verticalOffset, Vector2Int.up * (doorLength - 1) + Vector2Int.one));
        }
    }
}

public class GenRoom
{
    public GenRoomConnection firstConnection;
    public GenRoom globalNext;

    public GenRoomInfo info;

    public List<RectInt> unconnectedDoors;
    public RectInt rect;
    public uint generationIndex;
    public RoomType type;
    public string name;

    public GenRoom GetOtherRoom(GenRoomConnection connection)
    {
        GenConnection con = connection.connection;
        Debug.Assert(con.a != null && con.b != null, $"The room {GetHashCode()} is connected to a null room");
        GenRoom result = con.a;
        if (con.a == this)
        {
            result = con.b;
            Debug.Assert(con.b != this, $"The connection is self looping to the room: {GetHashCode()}");
        }
        else
            Debug.Assert(con.b == this, $"The connection doesn't belong to the room: {GetHashCode()}");
        return result;
    }
}

public class RoomCollection
{
    public List<GenRoomInfo> rooms;
    public Dictionary<RoomType, RangedInt> roomTypes;

    public void AddEdgarRooms(RoomType type, params Edgar.Unity.RoomTemplateSettings[] rooms)
    {
        Debug.Assert(!roomTypes.ContainsKey(type), $"The type: {type} has already been added");
        roomTypes[type] = new RangedInt(this.rooms.Count, this.rooms.Count + rooms.Length - 1);
        foreach (var room in rooms)
        {
            var rect = room.GetOutline().BoundingRectangle;
            var doors = room.GetComponent<Edgar.Unity.Doors>();
            GenRoomInfo info = new GenRoomInfo(rect.Width + 1, rect.Height + 1, doors.DoorsList.Count, room.name, room.gameObject);
            foreach (var door in doors.DoorsList)
            {
                Debug.Assert(rect.A.X < rect.B.X && rect.A.Y < rect.B.Y, $"({rect.A}, {rect.B})"); // A is always min and B is always max
                Vector2Int pos = door.From.ToVector3Int().ToVector2Int();
                pos.x -= rect.A.X; pos.y -= rect.A.Y;

                // NOTE: The door.To isn't neccessary greater than the door.From and they might be negative
                Vector2Int size = MathUtils.Abs((door.To - door.From).ToVector3Int().ToVector2Int()) + Vector2Int.one;
                info.doors.Add(new RectInt(pos, size));
            }
            this.rooms.Add(info);
        }
    }

    public void AddRooms(RoomType type, params GenRoomInfo[] rooms)
    {
        roomTypes[type] = new RangedInt(this.rooms.Count, this.rooms.Count + rooms.Length - 1);
        this.rooms.AddRange(rooms);
    }

    public GenRoomInfo GetRandomRoom(RoomType type)
    {
        return rooms[roomTypes[type].randomValue];
    }

    public RoomCollection(int roomCount, int typeCount)
    {
        rooms = new List<GenRoomInfo>(roomCount);
        roomTypes = new Dictionary<RoomType, RangedInt>(typeCount);
    }
}

public class LevelGenerator : MonoBehaviour
{
    public TileBase[] tiles;
    public TileBase doorTile;
    public Tilemap tilemap;

    public Edgar.Unity.RoomTemplateSettings[] normalRooms;
    public Edgar.Unity.RoomTemplateSettings[] hubRooms;
    public Edgar.Unity.RoomTemplateSettings startingRoom;
    public Edgar.Unity.RoomTemplateSettings bossRoom;
    public Edgar.Unity.RoomTemplateSettings shopRoom;

    GenRoom firstRoom;
    GenConnection firstConnection;
    RoomCollection collection;

    public static bool GenerateLayout(LevelGenerator generator, uint generationIndex)
    {
        Queue<GenRoom> rooms = new Queue<GenRoom>(4);
        List<GenRoom> genRooms = new List<GenRoom>(12);
        rooms.Enqueue(generator.firstRoom);
        while (rooms.Count > 0)
        {
            GenRoom room = rooms.Dequeue();
            if (room.generationIndex != generationIndex)
            {
                GenRoom firstGeneratedPrevRoom = null;
                int connectionCount = 0;
                for (GenRoomConnection connection = room.firstConnection; connection != null; connection = connection.next, connectionCount++)
                {
                    GenRoom otherRoom = room.GetOtherRoom(connection);
                    if (otherRoom.generationIndex != generationIndex)
                        rooms.Enqueue(otherRoom);
                    else if (firstGeneratedPrevRoom == null)
                        firstGeneratedPrevRoom = otherRoom;
                }

                GenRoomInfo roomInfo = generator.collection.GetRandomRoom(room.type);
                Debug.Assert(roomInfo.doors.Count >= connectionCount);
                // TODO: GetRandomRoom based on connectionCount

                if (firstGeneratedPrevRoom != null)
                {
                    List<GenRoom> connectedRooms = new List<GenRoom>(1)
                    {
                        firstGeneratedPrevRoom
                    };
                    List<Vector2Int> positions = GetUnconnectedPositions(firstGeneratedPrevRoom, roomInfo);
                    int positionCount = positions.Count;

                    for (GenRoomConnection connection = room.firstConnection; connection != null; connection = connection.next)
                    {
                        GenRoom otherRoom = room.GetOtherRoom(connection);
                        if (otherRoom.generationIndex == generationIndex && otherRoom != firstGeneratedPrevRoom)
                        {
                            connectedRooms.Add(otherRoom);
                            List<Vector2Int> otherPositions = GetUnconnectedPositions(otherRoom, roomInfo);

                            for (int i = positions.Count - 1; i >= 0; --i)
                                if (!otherPositions.Contains(positions[i]))
                                    positions.RemoveAt(i);

                            if (positions.Count == 0)
                            {
                                Debug.Log($"FAILED: Couldn't find any connected positions for the room: {room.name} from the {positionCount} started positions!");
                                return false;
                            }
                        }
                    }

                    RemoveCollidedPositions(genRooms, connectedRooms, positions, roomInfo.size);
                    if (positions.Count == 0)
                    {
                        if (positionCount == 0)
                            Debug.Log($"FAILED: Couldn't find any started positions for the room: {room.name} from the previous room: {firstGeneratedPrevRoom.name}");
                        else
                            Debug.Log($"FAILED: Couldn't find any uncollided positions  for the room: {room.name} from the {positionCount} started positions!");
                        return false;
                    }

                    Vector2Int randomPos = positions.RandomElement();
                    InitGenRoom(genRooms, room, roomInfo, randomPos, generationIndex);
                    for (GenRoomConnection connection = room.firstConnection; connection != null; connection = connection.next)
                    {
                        GenRoom otherRoom = room.GetOtherRoom(connection);
                        if (otherRoom.generationIndex == generationIndex)
                        {
                            RectInt door = GetUnconnectedMatchDoors(room, otherRoom).RandomElement();
                            connection.connection.rect = door;
                            Debug.Assert(room.unconnectedDoors.Remove(new RectInt(door.position - room.rect.position, door.size)));
                            Debug.Assert(otherRoom.unconnectedDoors.Remove(new RectInt(door.position - otherRoom.rect.position, door.size)));
                        }
                    }
                }
                else
                {
                    InitGenRoom(genRooms, room, roomInfo, Vector2Int.zero, generationIndex);
                }
            }
        }

        return true;

        static void InitGenRoom(List<GenRoom> genRooms, GenRoom room, GenRoomInfo info, Vector2Int pos, uint generationIndex)
        {
            room.info = info;
            room.rect = new RectInt(pos, info.size);
            room.unconnectedDoors = new List<RectInt>(info.doors);
            room.generationIndex = generationIndex;
            genRooms.Add(room);
        }

        static List<RectInt> GetUnconnectedMatchDoors(GenRoom a, GenRoom b)
        {
            List<RectInt> result = new List<RectInt>(1);
            foreach (RectInt doorA in a.unconnectedDoors)
                foreach (RectInt doorB in b.unconnectedDoors)
                    if (doorA.position + a.rect.position == doorB.position + b.rect.position && doorA.size == doorB.size)
                        result.Add(new RectInt(doorA.position + a.rect.position, doorA.size));
            Debug.Assert(result.Count > 0);
            return result;
        }

        static void RemoveCollidedPositions(List<GenRoom> rooms, List<GenRoom> connectedRooms, List<Vector2Int> positions, Vector2Int size)
        {
            foreach (GenRoom room in rooms)
                for (int i = positions.Count - 1; i >= 0; --i)
                    if (room.rect.OverlapWithoutBorder(new RectInt(positions[i], size), connectedRooms.Contains(room) ? 1 : 0))
                        positions.RemoveAt(i);
        }

        static List<Vector2Int> GetUnconnectedPositions(GenRoom room, GenRoomInfo roomInfo)
        {
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (RectInt unconnectedDoor in room.unconnectedDoors)
                foreach (RectInt door in roomInfo.doors)
                    if ((door.size == unconnectedDoor.size) && (roomInfo.size - door.position != room.rect.size - unconnectedDoor.position))
                        result.Add(unconnectedDoor.position + room.rect.position - door.position);

            return result;
        }
    }

    private void Start()
    {
#if false
        GenRoomInfo normal1  = new GenRoomInfo(10, 5, 8);
        GenRoomInfo normal2  = new GenRoomInfo(8, 12, 8);
        GenRoomInfo normal3  = new GenRoomInfo(10, 8, 8);
        normal1.AddSimpleDoors(2, 2);
        normal2.AddSimpleDoors(2, 2);
        normal3.AddSimpleDoors(2, 2);

        GenRoomInfo treasure = new GenRoomInfo(6, 6, 4);
        treasure.AddSimpleDoors(2, 2);

        GenRoomInfo hub1     = new GenRoomInfo(20, 10, 8);
        hub1.AddSimpleDoors(2, 3);
        GenRoomInfo hub2     = new GenRoomInfo(16, 24, 8);
        hub2.AddSimpleDoors(2, 4);

        GenRoomInfo boss     = new GenRoomInfo(20, 20, 3);
        boss.AddSimpleDoors(2, 4);

        collection = new RoomCollection(3 + 1 + 2 + 1, (int)RoomType.Count);
        collection.AddRooms(normal1, normal2, normal3);
        collection.AddRooms(treasure);
        collection.AddRooms(hub1, hub2);
        collection.AddRooms(boss);

        GenRoom n2 = AddRoom((int)RoomType.Normal, "n2");
        GenRoom n3 = AddRoom((int)RoomType.Normal, "n3");
        GenRoom n4 = AddRoom((int)RoomType.Normal, "n4");
        GenRoom n5 = AddRoom((int)RoomType.Normal, "n5");
        GenRoom n6 = AddRoom((int)RoomType.Normal, "n6");
        GenRoom n7 = AddRoom((int)RoomType.Normal, "n7");

        GenRoom h1 = AddRoom((int)RoomType.Hub, "h1");
        GenRoom h2 = AddRoom((int)RoomType.Hub, "h2");

        GenRoom t1 = AddRoom((int)RoomType.Treasure, "t1");
        GenRoom t2 = AddRoom((int)RoomType.Treasure, "t2");

        GenRoom b1 = AddRoom((int)RoomType.Boss, "b1");
        GenRoom n1 = AddRoom((int)RoomType.Normal, "n1");

        AddEdge(n1, h1);
        AddEdge(n2, h1);
        AddEdge(n3, h1);
        AddEdge(n4, h1);
        AddEdge(n3, t1);
        AddEdge(n4, n5);
        AddEdge(n4, h2);
        AddEdge(n5, b1);
        AddEdge(h2, n6);
        AddEdge(h2, n7);
        AddEdge(h2, t2);
#endif
        collection = new RoomCollection(normalRooms.Length + 3 + hubRooms.Length, (int)RoomType.Count);
        collection.AddEdgarRooms(RoomType.Normal, normalRooms);
        collection.AddEdgarRooms(RoomType.Shop, shopRoom);
        collection.AddEdgarRooms(RoomType.Hub, hubRooms);
        collection.AddEdgarRooms(RoomType.Boss, bossRoom);
        collection.AddEdgarRooms(RoomType.Starting, startingRoom);

        foreach (GenRoomInfo room in collection.rooms)
        {
            foreach (RectInt door in room.doors)
            {
                Debug.Assert(door.size == new Vector2Int(2, 1) || door.size == new Vector2Int(1, 2));
                Debug.Assert(door.position.x >= 0 && door.position.y >= 0);
            }
        }

        GenRoom n1 = AddRoom(RoomType.Normal, "n1");
        GenRoom n2 = AddRoom(RoomType.Normal, "n2");
        GenRoom n3 = AddRoom(RoomType.Normal, "n3");
        GenRoom n4 = AddRoom(RoomType.Normal, "n4");
        GenRoom n5 = AddRoom(RoomType.Normal, "n5");

        GenRoom h1 = AddRoom(RoomType.Hub, "h1");
        GenRoom h2 = AddRoom(RoomType.Hub, "h2");

        GenRoom shop = AddRoom(RoomType.Shop, "shop");
        GenRoom boss = AddRoom(RoomType.Boss, "boss");
        GenRoom start = AddRoom(RoomType.Starting, "start");

        AddEdge(start, n1);
        AddEdge(n1, h1);
        AddEdge(h1, n2);
        AddEdge(h1, n3);
        AddEdge(n2, n4);
        AddEdge(n3, h2);
        AddEdge(n4, boss);
        AddEdge(h2, n5);
        AddEdge(h2, shop);

        // NOTE: Generate level layout
        {
            const int maxTry = 512;
            bool success = false;
            uint i;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            for (i = 1; i < maxTry + 1; i++)
            {
                if (GenerateLayout(this, i))
                {
                    success = true;
                    break;
                }
            }
            watch.Stop();

            string result = success ? "has been successfully" : "couldn't be";
            Debug.Log("Layout" + result + $"generated after {i} tries in {watch.ElapsedMilliseconds} ms!");
        }

        // TODO: Generate room layout

        // NOTE: Spawn rooms
        {
            TileBase[] doorTiles = new TileBase[2].Populate(doorTile);
            TileBase[] tileArray = new TileBase[4096];
            for (GenRoom room = firstRoom; room != null; room = room.globalNext)
                tilemap.SetTilesBlock(room.rect.ToBoundsInt(), tileArray.Populate(tiles[(int)room.type]));
            for (GenConnection connection = firstConnection; connection != null; connection = connection.globalNext)
                tilemap.SetTilesBlock(connection.rect.ToBoundsInt(), new TileBase[2].Populate(doorTile));
            for (GenRoom room = firstRoom; room != null; room = room.globalNext)
            {
                var basePos = room.info.prefab.GetComponent<Edgar.Unity.RoomTemplateSettings>().GetOutline().BoundingRectangle.A;
                Vector3 pos = (Vector2)room.rect.position;
                pos.x -= basePos.X; pos.y -= basePos.Y;
                Instantiate(room.info.prefab, pos, Quaternion.identity);
            }
        }
    }

    private void Update()
    {
        for (GenRoom room = firstRoom; room != null; room = room.globalNext)
            GameDebug.DrawBox(room.rect, Color.green);
        for (GenConnection connection = firstConnection; connection != null; connection = connection.globalNext)
            GameDebug.DrawBox(connection.rect, Color.red);
    }

    GenRoom AddRoom(RoomType type, string name)
    {
        GenRoom room = new GenRoom
        {
            name = name,
            type = type,
            globalNext = firstRoom
        };
        firstRoom = room;
        return room;
    }

    void AddEdge(GenRoom a, GenRoom b)
    {
        GenConnection con = new GenConnection(a, b)
        {
            globalNext = firstConnection
        };
        firstConnection = con;

        GenRoomConnection aCon = new GenRoomConnection
        {
            next = a.firstConnection,
            connection = con
        };
        a.firstConnection = aCon;

        GenRoomConnection bCon = new GenRoomConnection
        {
            next = b.firstConnection,
            connection = con
        };
        b.firstConnection = bCon;
    }
}