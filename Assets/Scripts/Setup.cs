﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Setup : MonoBehaviour
{
    //Size of the grid
    public const int xsize = 10;
    public const int ysize = 24;
    //Initial movement speed
    private float moveWait = 1.0f;
    //Bounds of the grid
    public float leftwall;
    public float rightwall;
    public float bottom;
    public float top;
    public float stepsize;
    //Height and width of playerpiece
    public float currentheight;
    public float currentwidth;
    //Variables to hold tiles on the grid, pieces in the game, current playerpiece and next pieces
    public GameObject tile;
    public static GameObject[,] tiles;
    public GameObject[] pieces;
    public GameObject playerpiece;
    public GameObject next1;
    public GameObject next2;
    public GameObject next3;
    private GUIStyle guiStyle = new GUIStyle();
    //Parent object for tiles and pieces
    public GameObject board;
    //Flag if a taken tile has been hit
    public static bool hit = false;
    //Tiles currently occupied by playerpiece
    public static ArrayList currenttiles;
    //Sprites used for the grid
    public Sprite red;
    public Sprite pink;
    public Sprite blue;
    public Sprite lightblue;
    public Sprite green;
    public Sprite yellow;
    public Sprite alsored;
    public Sprite tilesprite;
    //Animation when clearing a row
    public GameObject clearanim;
    //Flags for if the piece can move, or if it is game over
    public bool canmove = false;
    public bool gameover = false;
    //Buffer for the long player piece
    private float longbuffer = 0;
    //Current score
    private int score;
    //Number of lines cleared
    private int linescleared;
    public UnityEngine.UI.Text finalscore;
    //Sounds used
    public AudioSource clearsound;
    public AudioSource rotatesound;
    public AudioSource changesound;
    //For blacking out the next pieces
    private bool blackedout;
    private float angle = 0f;
    // Start is called before the first frame update. Sets up the grid and calculated the bounds
    void Start()
    {
        score = 0;
        linescleared = 0;
        currenttiles = new ArrayList();
        tiles = new GameObject[xsize, ysize];
        stepsize = tile.GetComponent<SpriteRenderer>().bounds.size.x;
        leftwall = 1 - stepsize / 2;
        rightwall = leftwall + (stepsize * xsize);
        bottom = -3 - stepsize / 2;
        top = bottom + (stepsize * ysize);
        BuildGrid();
        StartGame();
        InvokeRepeating("MovePlayerPiece", moveWait, moveWait);
    }

    //Displays Next and the current score
    private void OnGUI()
    {
        if (!gameover)
        {
            guiStyle.fontSize = 20;
            GUI.Label(new Rect(120, 280, 100, 20), "Next", guiStyle);
            GUI.Label(new Rect(120, 150, 100, 20), "Score: " + score, guiStyle);
        }
        else
        {
            return;
        }
    }

    //Restart the level on button press
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Go back to the title screen on button press
    public void GoBack()
    {
        SceneManager.LoadScene(0);
    }

    //This method creates the first player piece and the next pieces coming in the correct position
    void StartGame()
    {
        int num = Random.Range(0, 7);
        playerpiece = Instantiate(pieces[num], new Vector3(leftwall + stepsize * 4, top - stepsize), Quaternion.identity);
        currentheight = playerpiece.GetComponentInChildren<SpriteRenderer>().bounds.extents.y;
        currentwidth = playerpiece.GetComponentInChildren<SpriteRenderer>().bounds.extents.x;
        playerpiece.transform.SetParent(board.transform);
        num = Random.Range(0, 7);
        next1 = Instantiate(pieces[num], new Vector3(leftwall - stepsize * 3, bottom + stepsize * 10, 0), Quaternion.identity);
        next1.transform.SetParent(board.transform);
        num = Random.Range(0, 7);
        next2 = Instantiate(pieces[num], new Vector3(leftwall - stepsize * 3, bottom + stepsize * 6, 0), Quaternion.identity);
        next2.transform.SetParent(board.transform);
        num = Random.Range(0, 7);
        next3 = Instantiate(pieces[num], new Vector3(leftwall - stepsize * 3, bottom + stepsize * 2, 0), Quaternion.identity);
        next3.transform.SetParent(board.transform);
        //If this is the innovated level run extra methods
        if (board.CompareTag("Innovation"))
        {
            float changenum = Random.Range(10.0f, 20.0f);
            Invoke("ChangePiece", changenum);
            Invoke("BlackOutNext", 1.0f);
            RotateCamera();
        }
    }

    //Update the score based on the number of lines cleared
    void UpdateScore(int multiplier)
    {
        score += 100 * (multiplier*2);
    }

    //When the current piece hits a taken tile or the bottom move to the next piece
    void CreatePlayerPiece()
    {
        longbuffer = 0;
        int num = Random.Range(0, 7);
        next1.transform.position = new Vector3(leftwall + stepsize * 4, top - stepsize, 0.0f);
        next2.transform.position = new Vector3(leftwall - stepsize * 3, bottom + stepsize * 10, 0.0f);
        next3.transform.position = new Vector3(leftwall - stepsize * 3, bottom + stepsize * 6, 0.0f);
        playerpiece = next1;
        next1 = next2;
        next2 = next3;
        next3 = Instantiate(pieces[num], new Vector3(leftwall - stepsize * 3, bottom + stepsize * 2, 0), Quaternion.identity);
        currentheight = playerpiece.GetComponentInChildren<SpriteRenderer>().bounds.extents.y;
        currentwidth = playerpiece.GetComponentInChildren<SpriteRenderer>().bounds.extents.x;
        playerpiece.transform.SetParent(board.transform);
        //If the is the innovated level the next pieces may have been set as inactive. If so set the playerpiece to active and the newly created next3 to inactive
        if (playerpiece.activeSelf == false)
        {
            playerpiece.SetActive(true);
            next3.SetActive(false);
        }
    }


    //Main player loop for every peice but Long
    void MainLoop()
    {
        //Check for player input and move the piece in the designated direction, checking it won't move outside of the grid
        if (Input.GetKeyDown(KeyCode.LeftArrow) && playerpiece.transform.GetChild(0).position.x - stepsize >= leftwall + 0.20)
        {
            currenttiles.Clear();
            playerpiece.transform.position = new Vector3(playerpiece.transform.position.x - stepsize, playerpiece.transform.position.y, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && playerpiece.transform.GetChild(0).position.x + stepsize <= rightwall - 0.20)
        {
            currenttiles.Clear();
            playerpiece.transform.position = new Vector3(playerpiece.transform.position.x + stepsize, playerpiece.transform.position.y, 0);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && playerpiece.transform.GetChild(0).position.y - stepsize * 2 >= bottom + 0.20)
        {
            currenttiles.Clear();
            playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y - stepsize, 0);
        }
        //Rotate the piece left if A is pressed, making sure that the new rotation won't make the piece outside of the grid
        else if (Input.GetKeyDown(KeyCode.A) && playerpiece.tag != "Box")
        {
            rotatesound.Play();
            currenttiles.Clear();
            playerpiece.transform.Rotate(new Vector3(0, 0, -90), Space.Self);
            //If the rotaion has made the piece outside the grid, push it back in
            if (playerpiece.transform.GetChild(0).position.x + currentwidth >= rightwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x - stepsize, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.x - currentwidth <= leftwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x + stepsize, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.y - currentheight <= bottom)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y + stepsize, 0);
            }
        }
        //Rotate the piece right if S is pressed, making sure that the new rotation won't make the piece outside of the grid
        else if (Input.GetKeyDown(KeyCode.S) && playerpiece.tag != "Box")
        {
            rotatesound.Play();
            currenttiles.Clear();
            playerpiece.transform.Rotate(new Vector3(0, 0, 90), Space.Self);
            //If the rotaion has made the piece outside the grid, push it back in
            if (playerpiece.transform.GetChild(0).position.x + currentwidth >= rightwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x - stepsize, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.x - currentwidth <= leftwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x + stepsize, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.y - currentheight <= bottom)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y + stepsize, 0);
            }
        }
        //Check is the piece has collided with the bottom if the grid or a taken tile
        if (CheckHit())
        {
            HitBottomOrCollide();
        }
    }

    //Main loop for the long piece
    void MainLoopLong()
    {
        //Check for player input and move the piece in the designated direction, checking it won't move outside of the grid
        if (Input.GetKeyDown(KeyCode.LeftArrow) && playerpiece.transform.GetChild(0).position.x - longbuffer >= leftwall + 0.20)
        {
            currenttiles.Clear();
            playerpiece.transform.position = new Vector3(playerpiece.transform.position.x - stepsize, playerpiece.transform.position.y, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && playerpiece.transform.GetChild(0).position.x + longbuffer<= rightwall - 0.20)
        {
            currenttiles.Clear();
            playerpiece.transform.position = new Vector3(playerpiece.transform.position.x + stepsize, playerpiece.transform.position.y, 0);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && playerpiece.transform.GetChild(0).position.y - stepsize * 2 + longbuffer >= bottom + 0.40)
        {
            currenttiles.Clear();
            playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y - stepsize, 0);
        }
        //Rotate the piece left if A is pressed, making sure that the new rotation won't make the piece outside of the grid
        else if (Input.GetKeyDown(KeyCode.A))
        {
            rotatesound.Play();
            currenttiles.Clear();
            playerpiece.transform.Rotate(new Vector3(0, 0, -90), Space.Self);
            //Update the space buffer when the long piece is rotated
            if (longbuffer == 0)
            {
                longbuffer = stepsize * 2;
            }
            else
            {
                longbuffer = 0;
            }
            //If the rotaion has made the piece outside the grid, push it back in
            if (playerpiece.transform.GetChild(0).position.x + currentwidth + longbuffer >= rightwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x - stepsize*2, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.x - currentwidth - longbuffer <= leftwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x + stepsize*2, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.y - currentheight <= bottom)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y + stepsize, 0);
            }
        }
        //Rotate the piece right if S is pressed, making sure that the new rotation won't make the piece outside of the grid
        else if (Input.GetKeyDown(KeyCode.S))
        {
            rotatesound.Play();
            currenttiles.Clear();
            playerpiece.transform.Rotate(new Vector3(0, 0, 90), Space.Self);
            //Update the space buffer when the long piece is rotated
            if (longbuffer == 0)
            {
                longbuffer = stepsize * 2;
            }
            else
            {
                longbuffer = 0;
            }
            //If the rotaion has made the piece outside the grid, push it back in
            if (playerpiece.transform.GetChild(0).position.x + currentwidth + longbuffer >= rightwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x - stepsize * 2, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.x - currentwidth - longbuffer <= leftwall)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x + stepsize * 2, playerpiece.transform.position.y, 0);
            }
            if (playerpiece.transform.GetChild(0).position.y - currentheight <= bottom)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y + stepsize, 0);
            }
        }
        //Check is the piece has collided with the bottom if the grid or a taken tile
        if (CheckHit())
        {
            HitBottomOrCollide();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate camera based on angle in the Innovated level
        Camera.main.transform.RotateAround(new Vector3(3, 1, 0), new Vector3(0, 0, angle), 10.0f * Time.deltaTime);
        //Run main loop if a piece has not collided and it's not game over
        if (!hit && !gameover){
            if (playerpiece.CompareTag("Long"))
            {
                MainLoopLong();
            }
            else
            {
                MainLoop();
            }
        }
    }

    //Using the current position of the playerpiece on the grid, check if it has collided with the bottom of the grid
    //or a tile that has already been taken
    bool CheckHit()
    {
        foreach (GameObject g in currenttiles)
        {
            if (g.GetComponent<Position>().y-1<0 || tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y - 1].CompareTag("Taken"))
            {
                return true;
            }
        }
        return false;
    }

    //Build the game grid with tiles and save them in the tiles array
    void BuildGrid()
    {
        float x = 1;
        float y = -3;
        for (int i = 0; i < ysize; i++)
        {
            for (int j = 0; j < xsize; j++)
            {
                GameObject newtile = Instantiate(tile, new Vector3(x, y, 0f), Quaternion.identity);
                newtile.GetComponent<Position>().x = j;
                newtile.GetComponent<Position>().y = i;
                newtile.GetComponent<Position>().current = false;
                tiles[j, i] = newtile;
                newtile.transform.SetParent(board.transform);
                x += tile.GetComponent<SpriteRenderer>().bounds.size.x;
            }

            y += tile.GetComponent<SpriteRenderer>().bounds.size.y;
            x = 1;
        }
    }

    //Checks if a row has been filled by running through the grid and checking if a row is full of tiles that are marked Taken
    void CheckLines()
    {
        //Number of lines that are filled
        int num = 0;
        //Array for the y position of the lines that are full
        ArrayList topcleared = new ArrayList();
        for (int i = 0; i < ysize; i++)
        {
            bool check = false;
            for (int j = 0; j < xsize; j++)
            {
                if (tiles[j, i].CompareTag("Taken"))
                {
                    check = true;
                }
                else
                {
                    check = false;
                    break;
                }
            }
            //If a row is filled clear the line
            if (check == true)
            {
                GameObject c = Instantiate(clearanim, new Vector3(leftwall+stepsize*5, bottom + stepsize * i + (stepsize/2), 0), Quaternion.identity);
                Destroy(c, 0.5f);
                topcleared.Add(i);
                num += 1;
                linescleared += 1;
                ClearLine(i);
                //Speed up as lines are cleared
                if (linescleared > 1 && moveWait >= 0.3f)
                {
                    moveWait -= 0.1f;
                    CancelInvoke("MovePlayerPiece");
                    InvokeRepeating("MovePlayerPiece", moveWait, moveWait);
                }
            }
        }
        //If lines were cleared drop the other blocks down if needed and update the score
        if (num > 0)
        {
            DropLines(num, topcleared);
            UpdateScore(num);
        }
    }

    //Clear a line by running through it and updating the sprite back to the normal tile sprite and update the tag
    void ClearLine(int y)
    {
        clearsound.Play();
        for (int i = 0; i < 10; i++)
        {
            tiles[i, y].tag = "Tile";
            tiles[i, y].GetComponent<SpriteRenderer>().sprite = tilesprite;
        }
    }

    //For every line that has been cleared, go from that line up and drop the taken tiles above it down
    void DropLines(int num, ArrayList topcleared)
    {

        foreach (int g in topcleared)
        {
            for (int i = g; i < ysize; i++)
            {
                for (int j = 0; j < xsize; j++)
                {
                    if (tiles[j, i].CompareTag("Taken"))
                    {
                        tiles[j, i - 1].tag = "Taken";
                        tiles[j, i - 1].GetComponent<SpriteRenderer>().sprite = tiles[j, i].GetComponent<SpriteRenderer>().sprite;
                        tiles[j, i].tag = "Tile";
                        tiles[j, i].GetComponent<SpriteRenderer>().sprite = tilesprite;
                    }
                }
            }
        }
    }

    //If the player piece has hit an already taken tile of the bottom of the grid
    public void HitBottomOrCollide()
    {
        hit = true;
        //Go through the occupied tiles and update the colour of the tile to that same as the playerpiece
        //Update the tag of the tile to Taken
        foreach (GameObject g in currenttiles)
        {
            tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].tag = "Taken";
            if (playerpiece.CompareTag("OtherSnake"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = red;
            }
            else if (playerpiece.CompareTag("Mountain"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = pink;
            }
            else if (playerpiece.CompareTag("Snake"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = blue;
            }
            else if (playerpiece.CompareTag("Box"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = lightblue;
            }
            else if (playerpiece.CompareTag("Lshape"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = green;
            }
            else if (playerpiece.CompareTag("OtherLShape"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = yellow;
            }
            else if (playerpiece.CompareTag("Long"))
            {
                tiles[g.GetComponent<Position>().x, g.GetComponent<Position>().y].GetComponent<SpriteRenderer>().sprite = alsored;
            }

        }
        //Destroy the player piece, clear the currenttiles
        currenttiles.Clear();
        Destroy(playerpiece);
        //Check if lines have been filled
        CheckLines();
        //Move on to the next piece
        CreatePlayerPiece();
        hit = false;
        //Check if the newly created piece has hit the top of the grid, and if so game over
        for (int i = 0; i < xsize; i++)
        {
            if (tiles[i, ysize - 1].CompareTag("Taken")){
                gameover = true;
                CancelInvoke("MovePlayerPiece");
                Destroy(board);
                Destroy(next1);
                Destroy(next2);
                Destroy(next3);
                OnGUI();
                finalscore.text = "Final Score: " + score;
            }
        }
    }


    //Moves the plaayer piece down once
    void MovePlayerPiece()
    {
        if (!hit)
        {
            currenttiles.Clear();
            if (playerpiece.transform.GetChild(0).position.y - stepsize >= bottom + 0.2)
            {
                playerpiece.transform.position = new Vector3(playerpiece.transform.position.x, playerpiece.transform.position.y - stepsize, 0);
            }
        }
    }

    //In the innovated level change the current player piece to a different piece at random intervals
    void ChangePiece()
    {
        changesound.Play();
        int num = Random.Range(0, 7);
        Vector3 currentpos = playerpiece.gameObject.transform.position;
        Destroy(playerpiece);
        playerpiece = Instantiate(pieces[num], currentpos, Quaternion.identity);
        float changenum = Random.Range(10.0f, 20.0f);
        Invoke("ChangePiece", changenum);
    }

    //In the innovated level black out the next pieces at random intervals
    void BlackOutNext()
    {
        if (blackedout)
        {
            next1.SetActive(true);
            next2.SetActive(true);
            next3.SetActive(true);
            blackedout = false;
        }
        else
        {
            next1.SetActive(false);
            next2.SetActive(false);
            next3.SetActive(false);
            blackedout = true;
        }
        float repeatrate = Random.Range(10.0f, 20.0f);
        Invoke("BlackOutNext", repeatrate);
    }

    //In the innovated level change the angle the camera rotates at random intervals
    void RotateCamera()
    {
        angle = Random.Range(-45.0f, 45.0f);
        float num = Random.Range(10.0f, 20.0f);
        Invoke("RotateCamera", num);
    }
}
