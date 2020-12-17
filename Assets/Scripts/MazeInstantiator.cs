//#define DEBUG_MODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Populates encompassing Unity scene with objects comprising a maze,
    which are encoded in a text file. In tandem creates structures to make
    accessible the state information on objects comprising the maze.

    Current objects include walls, foodpellets, and pacman, along with the
    floor of the maze. */
public class MazeInstantiator : MonoBehaviour
{
    // Text object encoding of maze to be loaded. TODO - UI for maze selection
    public TextAsset currentMaze;

    // Camera reference to adjust view parameters based on maze size.
    public GameObject cam;

    // String to be tokenized for maze structure extraction.
    private string rawMaze;

    // Tentative MxN maze dimensions, to be set during initial parse.
    private int RoomWidth = 1;
    private int RoomLength = 1;
    
    // @#$%#@^@ #$%^
    private int LayerHeight = 1;

    // Layer (in Unity y-coordinates) for objects of the maze to exist.
    private int ObjectLayerHeight = 1;

    /* Prefabs to be instantiated as objects of the maze */
    public GameObject pFloortile;
    public GameObject pWall;
    public GameObject pFoodpellet;
    public GameObject pPacsuit;

    /* State information of the maze, and work scope variables. */
    List<(int, int)> wallIndices;
    List<(int, int)> foodPelletIndices;
    bool[,] walls;
    bool[,] foodPellets;

    // Start is called before the first frame update
    void Start()
    {
        // n1 - populate through System.IO with a stream instead? or a db hmmm
        rawMaze = currentMaze.text;

        #if DEBUG_MODE
        Debug.Log(rawMaze);
        #endif

        // Prepare working variables for storing state information
        wallIndices = new List<(int, int)>();
        foodPelletIndices = new List<(int, int)>();

        // Instantiate maze objects
        Populate(rawMaze);

        walls = new bool[RoomWidth, RoomLength];
        foodPellets = new bool[RoomWidth, RoomLength];

        foreach((int,int) a in wallIndices) {
            walls[a.Item1, a.Item2] = true;
        }

        foreach((int,int) b in foodPelletIndices) {
            foodPellets[b.Item1, b.Item2] = true;
        }

        #if DEBUG_MODE
        Debug.Log(walls[7,4]);
        #endif
    }

    /* Sets RoomWidth and RoomLength for MxN maze while populating scene
      with object mappings from text symbols to prefabs.
      The floor of the maze is one unit of depth below the ObjectLayerHeight.
      State information structures are primed during this pass. */
    void Populate(string maze) {
        int rowIter = 0;
        int sequenceIter = 0;

        // Parse through encoded maze tokens.
        while(sequenceIter < maze.Length) {

            // Process one row of the maze.
            while(sequenceIter < maze.Length && !isEOL(maze[sequenceIter])) {
                // Extract column invariant from sequenceIter in current encoding
                int columnPos = sequenceIter - rowIter*RoomWidth;

                // Instantiate objects according to character -> prefab mappings, and record state info
                Spawn(maze[sequenceIter], columnPos, ObjectLayerHeight, rowIter);

                sequenceIter++;
            }

            // At EOL a full row has been reached and this line is achieved, though not achieved at EOF.
            rowIter++;

            // Advance past EOL
            sequenceIter++;
            RoomWidth = sequenceIter / rowIter;

            #if DEBUG_MODE
            Debug.Log(RoomWidth);
            #endif
        }

        // Create Ground
        RoomWidth -= 1;
        RoomLength = rowIter;
        GameObject ground = Instantiate(pFloortile, new Vector3(RoomWidth/2 - 0.5f, 0, RoomLength/2 - 0.5f), Quaternion.identity);
        ground.transform.localScale = new Vector3(RoomWidth, 1, RoomLength);

        // Set Camera
        SetCamera(ground);
    }

    void Spawn(char token, int x, int y, int z) {
        if (token == '%') {
            Instantiate(pWall, new Vector3(x, y, z), Quaternion.identity);
            wallIndices.Add((x, z));
        } else if (token == '.') {
            Instantiate(pFoodpellet, new Vector3(x, y, z), Quaternion.identity);
            foodPelletIndices.Add((x, z));
        } else if (token == 'P') {
            Instantiate(pPacsuit, new Vector3(x, y, z), Quaternion.identity);
        }
    }

    // Detects common text characters for End of Line.
    bool isEOL(char token) {
        if (token == '\n' || token == '\n') {
            #if DEBUG_MODE
            Debug.Log("EOL Hit");
            #endif

            return true;
        }
        return false;
    }

    void SetCamera(GameObject ground) {
        /*
        float mapX = RoomWidth;
        float mapY = RoomLength;

        float vertExtent = Camera.main.GetComponent<Camera>().orthographicSize;    
        float horzExtent = vertExtent * Screen.width / Screen.height;
        */

        cam.transform.position = new Vector3(ground.transform.position.x, cam.transform.position.y, ground.transform.position.z);
        cam.GetComponent<Camera>().orthographicSize = RoomWidth > RoomLength ? RoomWidth / 4.0f + 2 : RoomLength *
                                    Screen.height / (Screen.width * 4.0f) + 2; 
    }
}
