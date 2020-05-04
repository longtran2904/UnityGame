using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsSpawner : MonoBehaviour
{
    public GameObject prefab;

    private int[,] roomsPos = new int[5, 5];

    private string path = "Rooms";

    private List<Rooms> rooms = new List<Rooms>();

    private List<Rooms> leftRooms = new List<Rooms>();
    private List<Rooms> rightRooms = new List<Rooms>();
    private List<Rooms> upRooms = new List<Rooms>();
    private List<Rooms> bottomRooms = new List<Rooms>();
    private List<Rooms> ulRooms = new List<Rooms>();
    private List<Rooms> urRooms = new List<Rooms>();
    private List<Rooms> ubRooms = new List<Rooms>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Rooms room in Resources.LoadAll<Rooms>(path))
        {
            if (room.exits[0] == true)
            {
                leftRooms.Add(room);
            }

            if (room.exits[1] == true)
            {
                rightRooms.Add(room);
            }

            if (room.exits[3] == true)
            {
                bottomRooms.Add(room);
            }

            if (room.exits[0] == true && room.exits[2] == true)
            {
                ulRooms.Add(room);
            }

            if (room.exits[1] == true && room.exits[2] == true)
            {
                urRooms.Add(room);
            }

            if (room.exits[3] == true && room.exits[2] == true)
            {
                ubRooms.Add(room);
            }
        }

        //SpawnRooms(Random.Range(0, roomsPos.GetLength(0)), 0);

        //for (int x = 0; x < roomsPos.GetLength(0); x++)
        //{
        //    for (int y = 0; y < roomsPos.GetLength(1); y++)
        //    {
        //        if (roomsPos[x, y] != 0)
        //        {
        //            //Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        //            SetRooms();
        //        }
        //    }
        //}
    }

    void SpawnRooms(int _roomPosX, int _roomPosY)
    {
        int random = Random.Range(0, 5);

        if (((_roomPosX == 0 && (random == 0 || random == 1)) || (_roomPosX == roomsPos.GetLength(0) - 1 && (random == 2 || random == 3))) && _roomPosY == roomsPos.GetLength(1) - 1)
        {
            roomsPos[_roomPosX, _roomPosY] = 4;

            Debug.Log("X: " + _roomPosX + " Y: " + _roomPosY);

            return;
        }

        if (random == 0 || random == 1)
        {
            if (_roomPosX == 0)
            {
                roomsPos[_roomPosX, _roomPosY] = 3;
                SpawnRooms(_roomPosX, _roomPosY + 1);
                return;
            }

            //Debug.Log(_roomPosX + " " + _roomPosY);

            roomsPos[_roomPosX, _roomPosY] = 1;

            if (roomsPos[_roomPosX - 1, _roomPosY] != 0)
            {
                SpawnRooms(_roomPosX, _roomPosY);
                return;
            }

            SpawnRooms(_roomPosX - 1, _roomPosY);
        }
        else if (random == 2 || random == 3)
        {
            if (_roomPosX == roomsPos.GetLength(0) - 1)
            {
                roomsPos[_roomPosX, _roomPosY] = 3;
                SpawnRooms(_roomPosX, _roomPosY + 1);
                return;
            }

            //Debug.Log(_roomPosX + " " + _roomPosY);

            roomsPos[_roomPosX, _roomPosY] = 2;

            if (roomsPos[_roomPosX + 1, _roomPosY] != 0)
            {
                SpawnRooms(_roomPosX, _roomPosY);
                return;
            }

            SpawnRooms(_roomPosX + 1, _roomPosY);
        }
        else
        {
            if (_roomPosY == roomsPos.GetLength(1) - 1)
            {
                roomsPos[_roomPosX, _roomPosY] = 4;
                return;
            }

            //Debug.Log(_roomPosX + " " + _roomPosY);

            roomsPos[_roomPosX, _roomPosY] = 3;

            if (roomsPos[_roomPosX, _roomPosY + 1] != 0)
            {
                SpawnRooms(_roomPosX, _roomPosY);
                return;
            }

            SpawnRooms(_roomPosX, _roomPosY + 1);
        }
    }

    void SetRooms()
    {
        Rooms _roomToSpawn = new Rooms();

        for (int x = 0; x < roomsPos.GetLength(0); x++)
        {
            for (int y = 0; y < roomsPos.GetLength(1); y++)
            {
                int _roomType = roomsPos[x, y];

                if (_roomType == 1)
                {
                    if (roomsPos[x, y - 1] == 3)
                    {
                        _roomToSpawn = ulRooms[Random.Range(0, ulRooms.Count)];
                    }
                    else
                    {
                        _roomToSpawn = leftRooms[Random.Range(0, leftRooms.Count)];
                    }
                }
                else if (_roomType == 2)
                {
                    if (roomsPos[x, y - 1] == 3)
                    {
                        _roomToSpawn = urRooms[Random.Range(0, urRooms.Count)];
                    }
                    else
                    {
                        _roomToSpawn = rightRooms[Random.Range(0, leftRooms.Count)];
                    }
                }
                else if (_roomType == 3)
                {
                    _roomToSpawn = bottomRooms[Random.Range(0, leftRooms.Count)];
                }
            }
        }
    }
}