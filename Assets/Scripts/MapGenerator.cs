using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Sprite gridBackground;
    public Sprite spriteStart;
    public Sprite spriteExit;
    public Sprite spriteExitLocked;

    public Object prefabGhost;
    public Object prefabTreasure;
    public Object prefabHeart;
    public Object prefabKey;

    public GameObject tileBank;

    private Tile[,] grid;
    private List<Tile> tileList = new List<Tile>();
    private int nodeID = 1;
    private Vector2Int pos = Vector2Int.zero;

    private TilebankController tilebankController;
    private Tile tileStart;
    private Tile tileEnd;

    private int margin = 2;

    private Transform spriteContainer;
    private GameObject playerGO;
    private List<PlayerController> ghostList = new List<PlayerController>();

    private System.Random rnd;


    void Awake() {
        rnd = new System.Random();

        tilebankController = tileBank.GetComponent<TilebankController>();
        playerGO = GameObject.Find("player");
    }

    void Start()
    {
        init();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) {
            init();
        }

        updateCameraFOV(grid.GetLength(0) + margin, 9 + margin);

        //print(pixelPos2WorldPos( Input.mousePosition));

        // place selected tile on click
        if (Input.GetMouseButtonDown(0)) {
            Vector2 pp = pixelPos2WorldPos(Input.mousePosition);

            if (isEmpty(new Vector2Int((int)pp.x, (int)pp.y))) {

                PlayerController pl = playerGO.GetComponent<PlayerController>();

                // instantiate tile
                Sprite spr = tilebankController.getCurrentVariant().spriteList[tilebankController.getRotationIndex()];
                Tile tileTemplate = createTile((int)pp.x, (int)pp.y, spr);
                tileTemplate.canConnectAt = tilebankController.getCurrentVariant().canConnectAt[tilebankController.getRotationIndex()];
                tileTemplate.hasExtra = tilebankController.getCurrentVariant().hasExtra;
                spawnTile(tileTemplate);

                // add treasure when tile hasExtra==true
                if (tileTemplate.hasExtra) {
                    GameObject goTreasure = (GameObject)Object.Instantiate(prefabTreasure);
                    goTreasure.transform.position = new Vector3((int)pp.x, (int)pp.y, 0);
                    goTreasure.transform.SetParent(spriteContainer);
                    pl.addTarget(tileTemplate.getPosition());
                }

                // show next tile in tilebank
                tilebankController.nextRandomTileVariant();

                // check for valid path for player targets (i.e. finishTile, treasure, ...)
                checkPlayerPath();

                // check for valid ghost path towards player
                foreach (PlayerController ghost in ghostList) {
                    if (ghost != null) {
                        AStar astar = new AStar();
                        Vector3 ghostPos = ghost.gameObject.transform.position;
                        Node ghostNode = getNode(ghostPos);
                        Vector2 playerPos = pl.getPosition();
                        Node playerNode = getNode(new Vector2(Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.y)));
                        astar = new AStar();
                        List<Node> resultPath = astar.findPath(ghostNode, playerNode);
                        if (resultPath.Count > 0) {
                            ghost.setWaypoints(AStar.nodeList2posList(resultPath));
                        }
                    }
                }
            }
        }
    }

    public Node getNode(Vector2 pos) {
        return grid[(int)pos.x, (int)pos.y].node;
    }

    public static Vector2 pixelPos2WorldPos(Vector2 pixelPos) {
        Camera cam = Camera.main;

        Vector3 mousePos = pixelPos;

        Vector3 relSize = (mousePos / cam.pixelHeight);
        Vector3 gridPos = relSize * (cam.orthographicSize * 2);

        return new Vector2(gridPos.x, gridPos.y);
    }

    public void updateCameraFOV(int gridWidth, int gridHeight) {
        Camera cam = Camera.main;

        float aspect = (float)cam.pixelWidth / (float)cam.pixelHeight;

        int maxGridHeight = gridHeight;

        float zoom = (float)maxGridHeight / cam.orthographicSize;

        cam.orthographicSize = (float)maxGridHeight * 0.5f;

        cam.transform.position = new Vector3((float)(maxGridHeight)*aspect/zoom - 0.5f, (float)(maxGridHeight) / zoom - 0.5f, -1);
    }

    private void init() {
        WorldConstants.Instance.getGameManager().stage++;
        int currentStage = WorldConstants.Instance.getGameManager().stage;

        rnd = new System.Random();

        int gridWidth = Mathf.Min(5, 3 + rnd.Next(currentStage));
        int gridHeight = Mathf.Min(9, 4 + rnd.Next(currentStage));


        // clean
        if (spriteContainer != null) {
            Object.Destroy(spriteContainer.gameObject);
        }
        spriteContainer = new GameObject("sprite_container_" + rnd.Next(10000)).transform;
        spriteContainer.SetParent(this.transform);

        tileList.Clear();
        ghostList.Clear();

        PlayerController pl = playerGO.GetComponent<PlayerController>();
        pl.clearWaypoints();
        pl.clearTargets();

        playerGO.transform.position = Vector3.zero;

        grid = new Tile[gridWidth, gridHeight];
        for (int i = 0; i < grid.GetLength(0); i++) {
            for (int j = 0; j < grid.GetLength(1); j++) {
                grid[i, j] = null;

                GameObject goGridBG = createSpriteInstance(gridBackground, i, j);
                goGridBG.transform.position += Vector3.forward;
                goGridBG.transform.SetParent(spriteContainer);
            }
        }


        // create start and finish
        tileStart = spawnTile(createTile(0, 0, spriteStart));
        tileEnd = spawnTile(createTile(grid.GetLength(0) - 1, grid.GetLength(1) - 1, spriteExit));
        tileEnd.go.tag = "Finish";
        tileEnd.go.name = "Finish";
        CircleCollider2D endTileCollider = tileEnd.go.AddComponent<CircleCollider2D>();
        endTileCollider.isTrigger = true;
        endTileCollider.radius = 0.1f;


        // spawn random tiles, some with ghosts or treasures
        List<Vector2Int> possiblePos = new List<Vector2Int>();
        for (int u = 0; u < grid.GetLength(0); u++) {
            for (int v = 0; v < grid.GetLength(1); v++) {
                if ((u > 1 || v > 1) && (u < grid.GetLength(0) - 2 || v < grid.GetLength(1) - 2)) {
                    possiblePos.Add(new Vector2Int(u, v));
                }
            }
        }

        int numTiles = rnd.Next(3, (int)(grid.Length / 2));
        bool createdHeart = pl.hp == pl.maxHP;  // only spawn heart when players HP is not full
        bool createdKey = false;
        for (int i = 0; i < numTiles; i++) {
            // next random tile
            tilebankController.nextRandomTileVariant();

            // instantiate tile at random position
            int rndPosIdx = rnd.Next(0, possiblePos.Count);
            int rndRotation = rnd.Next(tilebankController.getCurrentVariant().spriteList.Count);
            int posX = possiblePos[rndPosIdx].x;
            int posY = possiblePos[rndPosIdx].y;

            if (isEmpty(new Vector2Int(posX, posY))) {
                Sprite spr = tilebankController.getCurrentVariant().spriteList[rndRotation];
                Tile tileTemplate = createTile(posX, posY, spr);
                tileTemplate.canConnectAt = tilebankController.getCurrentVariant().canConnectAt[rndRotation];
                spawnTile(tileTemplate);

                // spawn ghosts, treasures
                int chanceExtra = rnd.Next(100);
                if (chanceExtra < 50 && currentStage > 1) {
                    GameObject goGhost = (GameObject)Object.Instantiate(prefabGhost);
                    goGhost.transform.position = new Vector3(posX, posY, 0);
                    goGhost.transform.SetParent(spriteContainer);
                    PlayerController plGhost = goGhost.GetComponent<PlayerController>();
                    ghostList.Add(plGhost);
                } else if (chanceExtra < 60 && currentStage > 1) {
                    GameObject goTreasure = (GameObject)Object.Instantiate(prefabTreasure);
                    goTreasure.transform.position = new Vector3(posX, posY, 0);
                    goTreasure.transform.SetParent(spriteContainer);
                    pl.addTarget(tileTemplate.getPosition());
                } else if (!createdHeart) {
                    createdHeart = true;
                    GameObject goHeart = (GameObject)Object.Instantiate(prefabHeart);
                    goHeart.transform.position = new Vector3(posX, posY, 0);
                    goHeart.transform.SetParent(spriteContainer);
                    pl.addTarget(tileTemplate.getPosition());
                } else if (!createdKey && currentStage > 3) {
                    createdKey = true;
                    GameObject goKey = (GameObject)Object.Instantiate(prefabKey);
                    goKey.transform.position = new Vector3(posX, posY, 0);
                    goKey.transform.SetParent(spriteContainer);
                    pl.addTarget(tileTemplate.getPosition());

                    // lock exit, update tile sprite
                    Object.DestroyImmediate(tileEnd.go);

                    tileEnd.isLocked = true;
                    tileEnd.spr = spriteExitLocked;
                    tileEnd.go = createSpriteInstance(spriteExitLocked, tileEnd.gridX, tileEnd.gridY);
                    tileEnd.go.tag = "Finish";
                    CircleCollider2D lockedDoorCollider = tileEnd.go.AddComponent<CircleCollider2D>();
                    lockedDoorCollider.isTrigger = true;
                    lockedDoorCollider.radius = 0.1f;
                    tileEnd.go.transform.SetParent(spriteContainer);
                }
            }
        }

        if (!tileEnd.isLocked) {
            pl.addTarget(new Vector2(tileEnd.gridX, tileEnd.gridY));
        }
    }

    public void generateLevel() {
        init();
    }

    private void generatePath(int sx, int sy) {
        tileList.Clear();

        pos.x = sx;
        pos.y = sy;

        //spawnTile(spriteCrossX, pos.x, pos.y);

        // test next step direction
        List<StepDirection> pathDirections = new List<StepDirection>(new StepDirection[4] { StepDirection.XMINUS, StepDirection.XPLUS, StepDirection.YMINUS, StepDirection.YPLUS });
        shuffle<StepDirection>(pathDirections);

        while (tryStepAll(pathDirections) == true) {
            //spawnTile(spriteCrossX, pos.x, pos.y);


            // shuffle directions list for next iteration
            shuffle<StepDirection>(pathDirections);
        }


        
    }

    public enum StepDirection {
        XPLUS,
        XMINUS,
        YPLUS,
        YMINUS,
        RANDOM
    }

    private bool tryStepAll(List<StepDirection> directions) {
        foreach (StepDirection dir in directions) {
            if (step(dir) == true) {
                return true;
            }
        }

        return false;
    }

    private bool step(StepDirection stepDirection) {
        Vector2Int next = Vector2Int.zero;
        bool found = false;

        switch (stepDirection) {
            case StepDirection.XPLUS:
                next = pos + new Vector2Int(1, 0);
                found = true;
                break;
            case StepDirection.XMINUS:
                next = pos + new Vector2Int(-1, 0);
                found = true;
                break;
            case StepDirection.YPLUS:
                next = pos + new Vector2Int(0, 1);
                found = true;
                break;
            case StepDirection.YMINUS:
                next = pos + new Vector2Int(0, -1);
                found = true;
                break;
            case StepDirection.RANDOM:
                found = randomNeighborPosition(pos, ref next);
                break;
        }


        if (found) {
            if (isEmpty(next)) {
                pos = next;

                return true;
            }
        }

        return false;
    }

    private bool randomNeighborPosition(Vector2Int origin, ref Vector2Int neighbor) {
        Vector2Int next;

        List<int> dir = new List<int>();
        dir.Add(1);
        dir.Add(2);
        dir.Add(3);
        dir.Add(4);

        shuffle<int>(dir);


        foreach (int d in dir) {
            if (d == 1) {
                next = origin + new Vector2Int(1, 0) * 1;
            } else if (d == 2) {
                next = origin + new Vector2Int(0, 1) * 1;
            } else if (d == 3) {
                next = origin + new Vector2Int(-1, 0) * 1;
            } else {
                next = origin + new Vector2Int(0, -1) * 1;
            }

            if (isEmpty(next)) {
                neighbor = next;

                return true;
            }
        }

        return false;
    }

    private bool isInBounds(Vector2Int _pos) {
        if (_pos.x < 0 || _pos.y < 0)
            return false;

        if (_pos.x >= grid.GetLength(0) || _pos.y >= grid.GetLength(1))
            return false;

        return true;
    }

    private bool isEmpty(Vector2Int _pos) {
        if (isInBounds(_pos)) {

            if (grid[_pos.x, _pos.y] != null) {
                return false;
            }

            return true;
        }

        return false;
    }

    private void shuffle<T>(List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private List<int> shuffledRange(int from, int to) {
        List<int> list = new List<int>();
        for (int i = from; i <= to; i++) { list.Add(i); }
        shuffle<int>(list);

        return list;
    }

    private void linkTile(Tile t) {
        int x = t.gridX;
        int y = t.gridY;

        // look at N, E, S, W for adjacent tiles and connect them
        // only connections based on tile.canConnectAt[] array
        Vector2Int[] possibleNeighborPositions = new Vector2Int[4] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        List<Vector2Int> validNeighborPositions = new List<Vector2Int>();//new Vector2Int[4] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (char chr in t.canConnectAt.ToCharArray()) {
            int ownConntectID = int.Parse(chr.ToString());
            validNeighborPositions.Add(possibleNeighborPositions[ownConntectID]);
        }
        
        // connect neighbors
        foreach (Vector2Int nb in validNeighborPositions) {
            int cx = nb.x + x;
            int cy = nb.y + y;

            if (this.isInBounds(new Vector2Int(cx, cy))) {//(cx >= 0 && cx < grid.GetLength(0) && cy >= 0 && cy < grid.GetLength(1)) {
                if (grid[cx, cy] != null) {

                    // only allow when both connected node AND connecting node allow it (in canConnectAt)
                    //  0 and 2 can connect , 1 and 3 can connect together
                    string scn = t.canConnectAt;            // source connections
                    string tcn = grid[cx, cy].canConnectAt; // target connections
                    if ((scn.Contains("0") && tcn.Contains("2") && cy > y) || (scn.Contains("2") && tcn.Contains("0") && cy < y)
                        || (scn.Contains("1") && tcn.Contains("3") && cx > x) || (scn.Contains("3") && tcn.Contains("1") && cx < x)) {
                        t.node.addAdjacentNode(grid[cx, cy].node);
                        grid[cx, cy].node.addAdjacentNode(t.node);
                    }
                }
            }
        }

        if (this.isInBounds(new Vector2Int(x, y))) {
            grid[x, y] = t;
        }
    }

    private Tile spawnTile(Tile t) {
        if (this.isEmpty(new Vector2Int(t.gridX, t.gridY))) {
            //Tile t = createTile(template.gridX, template.gridY, template.spr);
            //t.canConnectAt = template.canConnectAt;
            t.node = createNode(t.gridX, t.gridY);
            linkTile(t);
            tileList.Add(t);

            return t;
        }

        return null;
    }

    private Node createNode(int posX, int posY) {
        nodeID = nodeID + 1;

        return new Node(nodeID, posX, posY);
    }

    public void checkPlayerPath() {
        PlayerController pl = playerGO.GetComponent<PlayerController>();
        List<Vector2> allTargets = new List<Vector2>(pl.getTargetList());

        Vector2 playerPos = pl.getPosition();
        Node startNode = getNode(new Vector2(Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.y)));

        foreach (Vector2 target in allTargets) {
            AStar astar = new AStar();
            
            Node targetNode = getNode(target);

            List<Node> resultPath = astar.findPath(startNode, targetNode);
            if (resultPath.Count > 0) {
                List<Vector2> poslist = AStar.nodeList2posList(resultPath);

                // assign waypoints to player and remove target from target list
                pl.addWaypoints(poslist);
                pl.removeTarget(target);

                // use last target as startNode for next path
                startNode = resultPath[resultPath.Count - 1];
            }
        }
    }

    public GameObject createSpriteInstance(Sprite spr, float posX, float posY) {
        GameObject go = new GameObject("sprite");
        SpriteRenderer r = go.AddComponent<SpriteRenderer>();
        r.sprite = spr;
        go.transform.localPosition = new Vector2(posX, posY);
        go.transform.SetParent(spriteContainer);

        return go;
    }

    public Tile createTile(int gridX, int gridY, Sprite spr) {
        Tile t = new Tile(gridX, gridY, spr);

        t.go = createSpriteInstance(spr, gridX, gridY);
        t.go.transform.SetParent(spriteContainer);

        return t;
    }

    public Tile getExitTile() {
        return tileEnd;
    }

    public class Tile {
        public Node node;
        public string canConnectAt = "0123";

        public bool hasExtra = false;

        public int gridX;
        public int gridY;
        public bool isLocked;
        public Sprite spr;
        public GameObject go;

        public Tile(int gridX, int gridY, Sprite spr) {
            this.gridX = gridX;
            this.gridY = gridY;
            this.spr = spr;
        }

        public Vector2 getPosition() {
            return new Vector2(gridX, gridY);
        }
    }
}
