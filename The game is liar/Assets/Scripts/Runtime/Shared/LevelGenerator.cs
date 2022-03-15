using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public struct Line
    {
        public Vector2Int start;
        public Vector2Int end;

        public Line(Vector2Int start, int length, bool isHorizontal)
        {
            this.start = start;
            end = start + (isHorizontal ? Vector2Int.right : Vector2Int.up) * length;
        }

        public Line(Vector2Int start, int offsetFromStart, int length, bool isHorizontal)
        {
            Vector2Int lineDir = isHorizontal ? Vector2Int.right : Vector2Int.up;
            this.start = start + lineDir * offsetFromStart;
            end = start + lineDir * length;
        }

        public Line Add(Vector2Int a)
        {
            Line newLine = this;
            newLine.start += a;
            newLine.end += a;
            return newLine;
        }

        public Vector2Int dir
        {
            get
            {
                if (start.x == end.x)
                    if (start.x == 0)
                        return Vector2Int.left;
                    else
                        return Vector2Int.right;
                else
                    if (start.y == 0)
                        return Vector2Int.down;
                    else
                        return Vector2Int.up;
            }
        }

        public bool IsConnected(Line other)
        {
            float dotProduct = dir.Dot(other.dir);
            return dotProduct == -1;
        }
    }

    public struct Room
    {
        public Vector2Int size;
        public Line[] doors;

        public Room(int width, int height, int doorCount)
        {
            size = new Vector2Int(width, height);
            doors = new Line[doorCount];
        }

        public void AddDoor(int index, Vector2Int pos, int length, bool isHorizontal)
        {
            doors[index].start = pos;
            doors[index].end = pos + (isHorizontal ? Vector2Int.up : Vector2Int.right) * length;
        }

        public void AddSimpleDoors(int cornerOffset, int doorLength)
        {
            Debug.Assert(doors.Length % 4 == 0);
            int doorsPerEdge = doors.Length / 4;

            Line[] edges = {
                new Line(Vector2Int.zero, size.x - 1, true),
                new Line(new Vector2Int(0, size.x - 1), size.y - 1, false),
                new Line(new Vector2Int(0, size.y - 1), size.x - 1, true),
                new Line(Vector2Int.zero, size.y - 1, false)
            };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < doorsPerEdge; ++j)
                {
                    bool isHorizontal = i % 2 == 0;
                    int dir = j < doorsPerEdge / 2 ? 1: -1;
                    Vector2Int pos = dir > 0 ? edges[i].start : edges[i].end;
                    int offset = (doorLength + cornerOffset) * j;
                    doors[i * doorsPerEdge + j] = new Line(pos, (cornerOffset + offset) * dir, doorLength * dir, isHorizontal);
                }
            }
        }
    }

    public struct Door
    {
        public Line line;
        public int connectedRoom;
    }

    public struct RoomInstance
    {
        public int roomBase;
        //public int roomType;
        public Vector2Int pos;
        public Door[] doors;
        public int doorCount;

        public RoomInstance(int roomBase, Vector2Int pos, int doorCount)
        {
            this.roomBase = roomBase;
            this.pos = pos;
            doors = new Door[doorCount];
            this.doorCount = 0;
        }
    }

    public struct RoomCollection
    {
        public Room[] rooms;
        private int roomCount;
        public RangedInt[] roomTypes;

        public RoomCollection(int roomCount, int typeCount)
        {
            rooms = new Room[roomCount];
            this.roomCount = 0;
            roomTypes = new RangedInt[typeCount];
        }

        public void AddRooms(int roomTypeIndex, params Room[] addedRooms)
        {
            roomTypes[roomTypeIndex] = new RangedInt(roomCount, addedRooms.Length + roomCount);
            addedRooms.CopyTo(rooms, roomCount);
            roomCount += addedRooms.Length;
        }
    }

    public struct Graph
    {
        public RoomCollection collection;
        public int[] nodes;
        public Vector2Int[] edges;

        public Graph(RoomCollection collection, int nodeCount, int edgeCount)
        {
            this.collection = collection;
            nodes = new int[nodeCount];
            edges = new Vector2Int[edgeCount];
        }

        public List<int> GetNeighbors(int node)
        {
            List<int> result = new List<int>(4);
            foreach (Vector2Int edge in edges)
            {
                if (edge.x == node)
                    result.Add(edge.y);
                else if (edge.y == node)
                    result.Add(edge.x);
            }
            return result;
        }

        public int GetNeighborCount(int node)
        {
            int count = 0;
            foreach (Vector2Int edge in edges)
                if (edge.x == node || edge.y == node)
                    ++count;
            return count;
        }

        public void AddNodes(int startIndex, int endIndex, int value)
        {
            for (int i = startIndex; i <= endIndex; ++i)
                nodes[i] = value;
        }

        public void AddEdge(int edgeIndex, int node1, int node2)
        {
            edges[edgeIndex] = new Vector2Int(node1, node2);
        }
    }

    public class Layout
    {
        public float energy;
        public float similarity;
        public RoomCollection collection;
        public RoomInstance[] roomInstances;
        public int roomCount;
        public Vector2Int[] connections;
        public int connectionCount;
        public List<Vector2Int> unconnectedEdges;

        public Layout()
        {
            energy = 0;
            similarity = 0;
            collection = new RoomCollection();
            roomInstances = null;
            roomCount = 0;
            connections = null;
            connectionCount = 0;
        }

        public Layout(RoomCollection roomCollection, int instanceCount, int connectionCount)
        {
            energy = 0;
            similarity = 0;
            collection = roomCollection;
            roomInstances = new RoomInstance[instanceCount];
            roomCount = 0;
            connections = new Vector2Int[connectionCount];
            this.connectionCount = 0;
            unconnectedEdges = new List<Vector2Int>(4);
        }

        public RectInt GetRect(int index)
        {
            return new RectInt(roomInstances[index].pos, collection.rooms[index].size);
        }

        /*public void RerollRoomInstance(int roomInstance)
        {
            roomInstances[roomInstance].roomBase = collection.roomTypes[roomInstances[roomInstance].roomType].randomValue;
        }*/

        public void AddRoom(Chain chain, int nodeIndex, int neighbor, int roomIndex, Vector2Int pos)
        {
            roomInstances[roomCount] = new RoomInstance(roomIndex, pos, chain.GetEdgeCount(nodeIndex));

            if (neighbor >= 0)
            {
                Room room = collection.rooms[roomIndex];
                roomInstances[roomCount].doorCount = 1;
                List<Line> doors = GetUnconnectedDoors(neighbor);
                int i;
                for (i = 0; i < doors.Count; ++i)
                {
                    Vector2Int doorPos = doors[i].start + roomInstances[neighbor].pos + doors[i].dir;
                    if (doorPos == pos)
                        break;
                    Debug.Assert(i < doors.Count - 1, $"Couldn't find any doors that are connected {neighbor} to {nodeIndex}!");
                }
                Line line = room.doors[i];
                roomInstances[roomCount].doors[0].line = line;
                roomInstances[neighbor].doors[roomInstances[neighbor].doorCount++].line = line.Add(pos + line.dir);
                connections[connectionCount++] = new Vector2Int(neighbor, roomCount);
                Debug.Assert(unconnectedEdges.Remove(new Vector2Int(neighbor, nodeIndex)), $"The unconnected edge {new Vector2Int(neighbor, nodeIndex)} isn't exist!");
            }

            foreach (Vector2Int edge in chain.edges)
                if (edge.x == nodeIndex)
                    unconnectedEdges.Add(new Vector2Int(roomCount, edge.y));
                else if (edge.y == nodeIndex)
                    unconnectedEdges.Add(new Vector2Int(roomCount, edge.x));

            ++roomCount;
        }

        public Room GetRoom(int roomInstance)
        {
            return collection.rooms[roomInstances[roomInstance].roomBase];
        }

        public List<int> GetNeighbors(int roomInstance)
        {
            List<int> neighbors = new List<int>(4);
            for (int i = 0; i < connectionCount; ++i)
                if (connections[i].x == roomInstance)
                    neighbors.Add(connections[i].y);
                else if (connections[i].y == roomInstance)
                    neighbors.Add(connections[i].x);
            return neighbors;
        }

        public bool WillRoomOverlap(Room room, Vector2Int pos)
        {
            RectInt rect = new RectInt(pos, room.size);
            for (int i = 0; i < roomCount; i++)
                if (rect.OverlapWithoutBorder(GetRect(i)))
                    return false;
            return true;
        }

        public List<Line> GetUnconnectedDoors(int roomIndex)
        {
            List<Line> result = new List<Line>();
            foreach (int neighbor in GetNeighbors(roomIndex))
            {
                Room room = GetRoom(neighbor);
                foreach (Line door in room.doors)
                {
                    foreach (Door usedDoor in roomInstances[roomIndex].doors)
                        if ((usedDoor.line.start == door.start && usedDoor.line.end == door.end) || (usedDoor.line.start == door.end && usedDoor.line.start == door.start))
                            goto CONTINUE;

                    result.Add(door);
                    CONTINUE:;
                }
            }

            return result;
        }

        public Layout Clone()
        {
            Layout newLayout = new Layout();
            newLayout.collection = collection;
            newLayout.roomInstances = (RoomInstance[])roomInstances.Clone();
            newLayout.roomCount = roomCount;
            newLayout.connections = (Vector2Int[])connections.Clone();
            newLayout.connectionCount = connectionCount;
            newLayout.unconnectedEdges = unconnectedEdges;
            return newLayout;
        }

        public Layout Clone(int dRoom, int dConnection)
        {
            Layout newLayout = new Layout();
            newLayout.roomInstances = new RoomInstance[roomInstances.Length + dRoom];
            roomInstances.CopyTo(newLayout.roomInstances, 0);
            newLayout.connections = new Vector2Int[connections.Length + dConnection];
            connections.CopyTo(newLayout.connections, 0);
            return newLayout;
        }
    }

    public class Chain
    {
        //public readonly Vector2Int[] connectedEdges;
        //public readonly int startNodeIndex;
        //public readonly int startEdgeIndex;
        public readonly int[] nodes;
        public readonly Vector2Int[] edges;
        public readonly bool fullLayout;

        /*public Chain(int connectedEdgeCount, int nodeCount, int edgeCount, int startNodeIndex, int startEdgeIndex, bool fullLayout)
        {
            //connectedEdges = new Vector2Int[connectedEdgeCount];
            nodes = new int[nodeCount];
            edges = new Vector2Int[edgeCount];
            //this.startNodeIndex = startNodeIndex;
            //this.startEdgeIndex = startEdgeIndex;
            this.fullLayout = fullLayout;
        }*/

        public Chain(int[] nodes, Vector2Int[] edges, bool fullLayout)
        {
            this.nodes = nodes;
            this.edges = edges;
            this.fullLayout = fullLayout;
        }

        public int GetEdgeCount(int node)
        {
            int result = 0;
            foreach (Vector2Int edge in edges)
                if (edge.x == node || edge.y == node)
                    ++result;
            return result;
        }

        /*public List<int> GetNodeNeighbors(int node)
        {
            List<int> neighbors = new List<int>(4);
            foreach (Vector2Int edge in edges)
                if (edge.x == node)
                    neighbors.Add(edge.y);
                else if (edge.y == node)
                    neighbors.Add(edge.x);
            return neighbors;
        }*/
    }
    public TileBase[] tiles;
    public Tilemap tilemap;

    enum RoomType
    {
        Normal,
        Treasure,
        Hub,
        Boss,

        Count
    }

    private void Start()
    {
        Room normal1  = new Room(10, 5, 8);
        normal1.AddSimpleDoors(2, 2);
        Room normal2  = new Room(8, 12, 8);
        normal2.AddSimpleDoors(2, 2);
        Room normal3  = new Room(10, 8, 8);
        normal3.AddSimpleDoors(2, 2);

        Room treasure = new Room(4, 4, 4);
        treasure.AddSimpleDoors(1, 2);

        Room hub1     = new Room(20, 10, 16);
        hub1.AddSimpleDoors(2, 2);
        Room hub2     = new Room(16, 24, 16);
        hub1.AddSimpleDoors(2, 2);

        Room boss     = new Room(20, 20, 3);
        boss.AddDoor(0, Vector2Int.up * 2, 2, false);
        boss.AddDoor(1, boss.size - Vector2Int.one - Vector2Int.down * 2, 2, false);
        boss.AddDoor(2, new Vector2Int(5, boss.size.y - 1), 2, true);

        RoomCollection collection = new RoomCollection(3 + 1 + 2 + 1, (int)RoomType.Count);
        collection.AddRooms((int)RoomType.Normal, normal1, normal2, normal3);
        collection.AddRooms((int)RoomType.Treasure, treasure);
        collection.AddRooms((int)RoomType.Hub, hub1, hub2);
        collection.AddRooms((int)RoomType.Boss, boss);

#if false
        Graph graph = new Graph(collection, 12, 13);
        graph.AddNodes(0, 6, (int)RoomType.Normal);
        graph.AddNodes(7, 9, (int)RoomType.Treasure);
        graph.AddNodes(9, 10, (int)RoomType.Hub);
        graph.nodes[11] = (int)RoomType.Boss;

        graph.AddEdge( 0,  0, 1);
        graph.AddEdge( 1,  1, 7);
        graph.AddEdge( 2,  1, 9);
        graph.AddEdge( 3,  9, 2);
        graph.AddEdge( 4,  9, 8);
        graph.AddEdge( 5,  9, 3);
        graph.AddEdge( 6,  9, 4);
        graph.AddEdge( 7, 10, 4);
        graph.AddEdge( 8, 10, 3);
        graph.AddEdge( 9, 10, 5);
        graph.AddEdge(10, 10, 6);
        graph.AddEdge(11, 11, 5);
        graph.AddEdge(12, 11, 6);

        Layout layout = GenerateLayout(graph);
#else
        Graph graph = new Graph(collection, 18, 17);
        graph.AddNodes( 0, 8, (int)RoomType.Normal);
        graph.AddNodes( 9, 12, (int)RoomType.Treasure);
        graph.AddNodes(13, 16, (int)RoomType.Hub);
        graph.nodes[17] = (int)RoomType.Boss;

        graph.AddEdge( 0,  0,  1);
        graph.AddEdge( 1,  1, 13);
        graph.AddEdge( 2, 13,  2);
        graph.AddEdge( 3, 13,  3);
        graph.AddEdge( 4, 13, 15);
        graph.AddEdge( 5, 15, 10);
        graph.AddEdge( 6, 15,  6);
        graph.AddEdge( 7,  6,  7);
        graph.AddEdge( 8,  6, 14);
        graph.AddEdge( 9, 14, 11);
        graph.AddEdge(10, 14,  8);
        graph.AddEdge(11,  3, 16);
        graph.AddEdge(12,  3,  4);
        graph.AddEdge(13, 16, 12);
        graph.AddEdge(14, 16,  5);
        graph.AddEdge(15,  4, 17);
        graph.AddEdge(16,  2,  9);

        //Layout layout = GeneratePlatformer(graph, 40, 100, 30);
#endif
        /*for (int i = 0; i < layout.roomInstances.Length; i++)
        {
            BoundsInt bounds = layout.GetRect(i).ToBoundsInt();
            tilemap.SetTilesBlock(bounds, new TileBase[bounds.Area()].Populate(tiles[i]));
        }*/
    }

#if false
    int n = 50;
    int m = 100;
    float t0 = .6f;
    float ratio = .2f / .6f;
    int maxFailedAttemps = 50;

    Layout GenerateLayout(Graph inputGraph)
    {
        Layout emptyLayout = new Layout();
        Stack<Layout> stack = new Stack<Layout>();
        stack.Push(emptyLayout);

        while (stack.Count > 0)
        {
            Layout layout = stack.Pop();
            Chain nextChain = GetNextChain(layout, inputGraph);
            List<Layout> partialLayouts = AddChain(layout, nextChain);

            if (partialLayouts != null)
            {
                if (nextChain.fullLayout)
                    return partialLayouts[0];
                else
                    foreach (Layout partialLayout in partialLayouts)
                        stack.Push(partialLayout);
            }
        }

        return null;
    }

    Chain GetNextChain(Layout layout, Graph graph)
    {
        /*List<Chain> chains = new List<Chain>();
        List<Chain> faces = GetFacesFromGraph(graph);
        int depth = 0;

        while (!IsFullChain(graph))
        {
            if (chains.Count == 0)
            {
                Chain smallestFace = GetSmallestFace(faces);
                faces.Remove(smallestFace);
                chains.Add(smallestFace);
                smallestFace.depth = depth;
            }
            else
            {
                Chain neighbor = GetNeighborChain(chains, faces);
                if (neighbor)
                {
                    faces.Remove(neighbor);
                    chains.Add(neighbor);
                    neighbor.depth = depth;
                }
                else
                {
                    int node = GetUnvisitedNeighborFromSmallestDepthChain(chains);
                    List<int> nodes = new List<int>();
                    nodes.Add(node);

                    while (NodeHasUnvisitedNeighbors(node, chains))
                    {
                        int newNode = GetRandomUnvisitedNeighbor(node, chains);
                        nodes.Add(newNode);

                        if (IsThisNodeProcessed(newNode, chains))
                            break;

                        node = newNode;
                    }

                    AddNodes(nodes, chains, depth);
                }
            }

            depth++;
        }*/

        return null;
    }

    List<Layout> AddChain(Layout layout, Chain chain)
    {
        Layout currentLayout = GetInitialLayout(layout, chain);
        List<Layout> generatedLayouts = new List<Layout>();
        generatedLayouts.Add(currentLayout);
        float t = t0;
        float totalDeltaE = 0;
        int failedAttemps = 0;

        for (int i = 1; i <= n; i++)
        {
            if (failedAttemps > maxFailedAttemps)
                break;
            bool iterationSuccessful = false;

            for (int j = 1; j <= m; j++)
            {
                Layout newLayout = PerturbLayout(currentLayout, chain);

                if (IsValid(newLayout))
                    if (DifferentEnough(newLayout, generatedLayouts))
                    {
                        iterationSuccessful = true;
                        generatedLayouts.Add(newLayout);
                        if (chain.fullLayout)
                            return generatedLayouts;
                    }

                float dE = newLayout.energy - currentLayout.energy;
                totalDeltaE += dE;
                float k = totalDeltaE / (i * j);

                if (dE < 0)
                    currentLayout = newLayout;
                else if (Random.value < Mathf.Exp(dE / (k + t)))
                    currentLayout = newLayout;
            }

            if (!iterationSuccessful)
                ++failedAttemps;

            t *= ratio;
        }

        return generatedLayouts;
    }

    Layout GetInitialLayout(Layout layout, Chain chain)
    {
        Layout newLayout = layout.Clone(chain.nodes.Length, chain.edges.Length + chain.connectedEdges.Length);
        for (int edgeIndex = 0; edgeIndex < chain.edges.Length; ++edgeIndex)
            newLayout.connections[edgeIndex + chain.startEdgeIndex] = chain.edges[edgeIndex] + Vector2Int.one * chain.startNodeIndex;
        for (int edgeIndex = 0; edgeIndex < chain.connectedEdges.Length; ++edgeIndex)
            newLayout.connections[edgeIndex + chain.startEdgeIndex + chain.edges.Length] = chain.connectedEdges[edgeIndex] + new Vector2Int(chain.startNodeIndex, 0);

        int[] nodes = new int[chain.nodes.Length];
        BFS(nodes, chain);
        int i = chain.startNodeIndex;
        foreach (int node in nodes)
        {
            newLayout.roomInstances[i].roomType = chain.nodes[node];
            newLayout.RerollRoomInstance(i);
            // Init roomInstance.doors

            List<Room> neighborRooms = newLayout.GetNeighborRooms(i);
            if (neighborRooms == null)
                continue;
            List<Vector2Int> allPos = GetConfigSpaces(newLayout.GetRoom(i), neighborRooms);
            float minEnergy = 99999f;
            Vector2Int bestPos = allPos[0];
            foreach (Vector2Int pos in allPos)
            {
                newLayout.roomInstances[i].pos = pos;
                float energy = GetLayoutEnergy(newLayout);
                if (energy < minEnergy)
                {
                    minEnergy = energy;
                    bestPos = pos;
                }
            }
            // newLayout.energy += minEnergy / __some_Constant__
            newLayout.roomInstances[i].pos = bestPos;

            ++i;
        }

        newLayout.energy = GetLayoutEnergy(newLayout);
        CenterLayout(newLayout);
        return newLayout;
    }

    void BFS(int[] nodes, Chain chain)
    {
        bool[] marked = new bool[chain.nodes.Length];
        Queue<int> queue = new Queue<int>(chain.connectedEdges.Length * 2);
        foreach (Vector2Int connectedEdge in chain.connectedEdges)
            queue.Enqueue(connectedEdge.x);

        int i = 0;
        while (queue.Count > 0)
        {
            int node = queue.Dequeue();
            if (!marked[node])
            {
                nodes[i++] = node;
                marked[node] = true;
                foreach (int neighbor in chain.GetNodeNeighbors(node))
                    if (!marked[neighbor])
                        queue.Enqueue(neighbor);
            }
        }
    }

    Layout PerturbLayout(Layout layout, Chain chain)
    {
        bool changeShape = MathUtils.RandomBool(.4f);
        int randomRoom = Random.Range(chain.startNodeIndex, layout.roomInstances.Length);
        Layout newLayout = layout.Clone();

        if (changeShape)
            newLayout.RerollRoomInstance(randomRoom);
        else
            // TODO: Don't repick the same pos
            newLayout.roomInstances[randomRoom].pos = GetConfigSpaces(newLayout.GetRoom(randomRoom), newLayout.GetNeighborRooms(randomRoom)).RandomElement();

        newLayout.energy = GetLayoutEnergy(newLayout);
        return newLayout;
    }

    bool IsValid(Layout newLayout)
    {
        for (int i = 0; i < newLayout.roomInstances.Length; ++i)
            for (int j = 0; i < newLayout.roomInstances.Length; ++j)
                if (i != j)
                    if (MathUtils.CollideArea(newLayout.GetRect(i), newLayout.GetRect(j)) != 0f)
                        return false;

        foreach (Vector2Int edge in newLayout.connections)
            if (!newLayout.GetRect(edge.x).Overlaps(newLayout.GetRect(edge.y)))
                return false;

        return true;
    }

    bool DifferentEnough(Layout newLayout, List<Layout> generatedLayouts)
    {
        CenterLayout(newLayout);

        newLayout.similarity = 0;
        for (int i = 0; i < newLayout.roomInstances.Length; i++)
            newLayout.similarity += (newLayout.GetRect(i).position - generatedLayouts[0].GetRect(i).position).sqrMagnitude;

        for (int i = 1; i < generatedLayouts.Count; ++i)
            if (newLayout.similarity < generatedLayouts[i].similarity)
                return false;

        return true;
    }

    void CenterLayout(Layout layout)
    {
        RectInt rect = layout.GetRect(0);
        for (int i = 0; i < layout.roomInstances.Length; i++)
        {
            rect.min = new Vector2Int(Mathf.Min(rect.min.x, layout.GetRect(i).min.x), Mathf.Min(rect.min.y, layout.GetRect(i).min.y));
            rect.max = new Vector2Int(Mathf.Max(rect.max.x, layout.GetRect(i).max.x), Mathf.Max(rect.max.y, layout.GetRect(i).max.y));
        }

        if (rect.center == Vector2.zero)
            return;

        Vector2Int dir = (-rect.center).ToVector2Int();
        for (int i = 0; i < layout.roomInstances.Length; ++i)
            layout.roomInstances[i].pos += dir;
    }

    // TODO: This is a O(n2) algorithm. We can replace this by exploiting the fact that we are always perturbing only one node at a time.
    // First, we store the energy on each indiviual node. Then we update each node's energy every time we perturb the layout.
    float GetLayoutEnergy(Layout layout)
    {
        float A = 0, D = 0;

        float totalArea = 0;
        for (int i = 0; i < layout.roomInstances.Length; ++i)
            totalArea += layout.GetRect(i).Area();
        float w = totalArea / layout.roomInstances.Length * 100f;

        for (int i = 0; i < layout.roomInstances.Length; ++i)
            for (int j = 0; j < layout.roomInstances.Length; ++j)
                if (i != j)
                    A += MathUtils.CollideArea(layout.GetRect(i), layout.GetRect(j));

        foreach (Vector2Int connection in layout.connections)
        {
            RectInt a = layout.GetRect(connection.x);
            RectInt b = layout.GetRect(connection.y);
            if (!a.Overlaps(b))
                D += (a.center - b.center).sqrMagnitude;
        }

        return Mathf.Exp(A / w) * Mathf.Exp(D / w) - 1;
    }

    List<Vector2Int> GetConfigSpaces(Room room, List<Room> neighborRooms)
    {
        List<Vector2Int> allPos = new List<Vector2Int>(neighborRooms.Count * 2);
        foreach (Room neighbor in neighborRooms)
            foreach (Room.Line neighborLine in neighbor.doors)
                foreach (Room.Line line in room.doors)
                    if (line.IsConnected(neighborLine))
                        allPos.Add(neighborLine.start - line.start);
        return allPos;
    }
#endif
#if false
    public Layout GeneratePlatformer(Graph inputGraph, int maxNodesPerChain, int numberOfAttemptsTotal, int numberOfAttemptsPerNode)
    {
        Layout layout = new Layout(inputGraph.collection, inputGraph.nodes.Length, inputGraph.edges.Length);
        bool[] marked = new bool[inputGraph.nodes.Length];

        while (layout != null)
        {
            Chain nextChain = GetNextChain(layout, inputGraph, marked, maxNodesPerChain);
            layout = EvolveChain(layout, nextChain, numberOfAttemptsTotal, numberOfAttemptsPerNode);
            // TODO: backtrack to the previous layout if can't evolve the current chain
            if (nextChain.fullLayout)
                break;
        }
        return layout;
    }

    Chain GetNextChain(Layout layout, Graph graph, bool[] marked, int maxNodesPerChain)
    {
        int startNode = -1;

        if (layout.unconnectedEdges.Count == 0)
        {
            // Find an arbitrary leaf node
            for (int i = 0; i < graph.nodes.Length; ++i)
            {
                int count = graph.GetNeighborCount(i);
                if (count == 1)
                {
                    startNode = i;
                    break;
                }
                Debug.Assert(count != 0, $"Node {i} is unconnected!");
            }
        }
        else
        {
            startNode = layout.unconnectedEdges.PopRandom().y;
        }

        if (startNode < 0)
        {
            Debug.LogError("There aren't any leaf nodes!");
            return null;
        }

        // BFS to group nodes into smaller chain
        List<int> nodes = new List<int>(maxNodesPerChain);
        List<Vector2Int> edges = new List<Vector2Int>(maxNodesPerChain - 1);

        Queue<int> queue = new Queue<int>(4);
        queue.Enqueue(startNode);

        while (queue.Count > 0 && nodes.Count <= maxNodesPerChain)
        {
            int node = queue.Dequeue();
            if (!marked[node])
            {
                nodes.Add(node);
                marked[node] = true;
                foreach (int neighbor in graph.GetNeighbors(node + layout.roomCount))
                {
                    if (!marked[neighbor])
                        queue.Enqueue(neighbor);
                    if (!(edges.Contains(new Vector2Int(node, neighbor)) || edges.Contains(new Vector2Int(neighbor, node)))) // This is so bad!
                        edges.Add(new Vector2Int(node, neighbor));
                }
            }
        }

        Chain chain = new Chain(nodes.ToArray(), edges.ToArray(), nodes.Count + layout.roomCount == layout.roomInstances.Length);
        return chain;
    }

    public Layout EvolveChain(Layout initialLayout, Chain chain, int numberOfAttemptsTotal, int numberOfAttemptsPerNode)
    {
        for (int i = 0; i < numberOfAttemptsTotal; i++)
        {
            Layout layout = initialLayout.Clone();
            for (int nodeIndex = 0; nodeIndex < chain.nodes.Length; nodeIndex++)
                if (!TryLayoutNode(layout, chain, nodeIndex, numberOfAttemptsPerNode))
                    goto CONTINUE;

            return layout;
            CONTINUE:;
        }

        return null;
    }

    public bool TryLayoutNode(Layout layout, Chain chain, int nodeIndex, int numberOfAttemptsPerNode)
    {
        if (nodeIndex == 0)
        {
            layout.AddRoom(chain, 0, -1, layout.collection.roomTypes[chain.nodes[nodeIndex]].randomValue, Vector2Int.zero);
            return true;
        }

        for (int i = 0; i < numberOfAttemptsPerNode; i++)
        {
            int room = layout.collection.roomTypes[chain.nodes[nodeIndex]].randomValue;
            int neighbor = GetConnectedNeighbor(layout, nodeIndex);
            Debug.Assert(neighbor >= 0, $"The node: {nodeIndex} has an invalid neighbor : {neighbor}.");
            List<Vector2Int> validPos = GetConfigSpaces(layout.collection.rooms[room], layout, neighbor);
            if (validPos.Count > 0)
            {
                Vector2Int pos = validPos.RandomElement();
                layout.AddRoom(chain, nodeIndex, neighbor, room, pos);
                return true;
            }
        }

        return false;
    }

    public int GetConnectedNeighbor(Layout layout, int nodeIndex)
    {
        foreach (Vector2Int edge in layout.unconnectedEdges)
            if (edge.y == nodeIndex)
                return edge.x;
            else if (edge.x == nodeIndex)
                Debug.LogError("Unconnected edges aren't initialized correctly!");
        return -1;
    }

    public List<Vector2Int> GetConfigSpaces(Room room, Layout layout, int neighbor)
    {
        List<Vector2Int> result = new List<Vector2Int>(4);
            foreach (Line neighborDoor in layout.GetUnconnectedDoors(neighbor))
                foreach (Line door in room.doors)
                    if (neighborDoor.IsConnected(door))
                    {
                        // NOTE: the neighborDoor and door may have start and end switched
                        Vector2Int pos = layout.roomInstances[neighbor].pos + neighborDoor.start - door.start + neighborDoor.dir;
                        if (!layout.WillRoomOverlap(room, pos))
                            if (!result.Contains(pos))
                                result.Add(pos);
                    }

        return result;
    }
#endif
}