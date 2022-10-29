using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapObject : MonoBehaviour
{
    public Material normalSpaceMaterial;
    public Material possibleSpaceMaterial;
    public Material impossibleSpaceMaterial;

    public Material extraSpaceMaterial;
    public Material[] questStartSpaceMaterials;
    public Material[] questEndSpaceMaterials;

    public Material[] playerMaterials;

    public string[] keyNames;
    public int keyNameIndex = 0;

    int spaceIndex = 0;
    public Dictionary<string, GameObject[]> paths;
    List<GameObject> possibleSpaces = new List<GameObject>();
    public GameObject[] extraCardSpaces;
    public GameObject[] questStartSpaces;
    public GameObject[] questEndSpaces;
    public Transform boardSpacesParent;

    public GameObject[] players;
    int playerTurn;
    public int PlayerTurn
    {
        set
        {
            playerTurn = value;

            turnText.color = playerMaterials[value].color;
        }
        get { return playerTurn; }
    }
    PlayerScript[] ps;
    public Vector3[] playerOffsets;

    int gameRound = 0;
    int enemyAmount = 0;
    int playerCombatPoints;
    public int enemyIncrease = 2;
    bool enemyPhase = false;
    int GameRound
    {
        set
        {
            gameRound = value;
            enemyAmount = (value + enemyIncrease - 1) / enemyIncrease;
            enemyAmountText.text = "Number Of Enemies: " + enemyAmount;

            PlayerTurn = 0;
        }
        get { return gameRound; }
    }
    int rollMoves;

    public TextMeshProUGUI rollText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI enemyAmountText;
    public Button rollButton;
    public TextMeshProUGUI[] playerCardsText;

    public GameObject extraCardSpaceText;
    public Vector3 extraCardTextOffset;

    void Awake()
    {
        paths = new Dictionary<string, GameObject[]>();
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Space>() == null) //adding Space Script
            {
                child.gameObject.AddComponent<Space>().mo = this;
            }
            else //initializing paths dictionary
            {
                Space space = child.gameObject.GetComponent<Space>();
                if (!paths.ContainsKey(space.pathKey)) //add a new path in the paths dictionary
                {
                    paths.Add(space.pathKey, new GameObject[30]);
                }
                paths[space.pathKey][space.spaceIndex] = child.gameObject; //add to paths dictionary
                space.PossibleSpace = false; //prevent showing all spaces as possible on the first turn

                //set the board spaces color depending on what type of space it is
                Transform boardSpaceTransform = boardSpacesParent.Find(space.pathKey + space.spaceIndex);
                MeshRenderer boardSpaceRenderer = boardSpaceTransform.GetComponent<MeshRenderer>();

                if (IsSpecialSpace(space.pathKey, space.spaceIndex, extraCardSpaces)) //extra card space
                    boardSpaceRenderer.material = extraSpaceMaterial;
                else if (IsSpecialSpace(space.pathKey, space.spaceIndex, questStartSpaces)) //quest start space
                    boardSpaceRenderer.material = questStartSpaceMaterials[WhichQuest(space.pathKey, space.spaceIndex)];
                else if (IsSpecialSpace(space.pathKey, space.spaceIndex, questEndSpaces)) //quest end space
                    boardSpaceRenderer.material = questEndSpaceMaterials[WhichQuest(space.pathKey, space.spaceIndex)];
                else //normal space
                    boardSpaceRenderer.material = normalSpaceMaterial;
            }
        }

        //find all the spaces where they have multiple path options
        for (int i = 1; i < keyNames.Length; i++) //skipping main path
        {
            //START OF SIDE PATHS
            Vector3 startOfPathObject = paths[keyNames[i]][0].transform.position; //first space of path
            Transform closestChild = null;
            float distance = 20;
            foreach (Transform child in transform)
            {
                if(keyNames[i] != child.GetComponent<Space>().pathKey 
                   && Vector3.Distance(child.position, startOfPathObject) < distance)
                {
                    distance = Vector3.Distance(child.position, startOfPathObject);
                    closestChild = child;
                }
            }
            Space closestChildSpace = closestChild.GetComponent<Space>();

            closestChildSpace.startOfConnectedPath = true;
            if (closestChildSpace.connectedPath1 == "")
                closestChildSpace.connectedPath1 = keyNames[i];
            else
                closestChildSpace.connectedPath2 = keyNames[i];

            //END OF SIDE PATHS
            int lastSpaceIndex = FindEndOfPath(keyNames[i]); //last of the side path
            Vector3 endOfPathObject = paths[keyNames[i]][lastSpaceIndex].transform.position;
            distance = 20; //starting distance
            closestChild = null; //starting child
            foreach (Transform child in transform)
            {
                if (keyNames[i] != child.GetComponent<Space>().pathKey
                   && Vector3.Distance(child.position, endOfPathObject) < distance)
                {
                    distance = Vector3.Distance(child.position, endOfPathObject);
                    closestChild = child;
                }
            }
            //make truck route skip to the end of the map
            if(i == 1) //truck route path
            {
                Space endOfMainPathSpace = paths[keyNames[0]][29].GetComponent<Space>();
                endOfMainPathSpace.connectedPath1 = keyNames[i];
                endOfMainPathSpace.startOfConnectedPath = false;
            }
            else //any other path
            {
                closestChildSpace = closestChild.GetComponent<Space>();

                closestChildSpace.startOfConnectedPath = false;
                if (closestChildSpace.connectedPath1 == "")
                    closestChildSpace.connectedPath1 = keyNames[i];
                else
                    closestChildSpace.connectedPath2 = keyNames[i];
            }
        }
    }

    private void Start()
    {
        boardSpacesParent.gameObject.SetActive(true);

        ps = new PlayerScript[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            ps[i] = players[i].GetComponent<PlayerScript>();
            ps[i].pathKey = keyNames[0];
            ps[i].spaceIndex = 0;
            ps[i].playerNum = i;
            players[i].transform.position = paths[keyNames[0]][0].transform.position + playerOffsets[i]; //starting position
            playerCardsText[i].color = playerMaterials[i].color;
        }
        GameRound++;
        phaseText.text = "Move Phase";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) //for initializing space variables
        {
            spaceIndex = 0;
            keyNameIndex++;
        }

        if (enemyPhase)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
                RemoveCard(PlayerTurn, 1 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
                RemoveCard(PlayerTurn, 2 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
                RemoveCard(PlayerTurn, 3 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
                RemoveCard(PlayerTurn, 4 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
                RemoveCard(PlayerTurn, 5 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6))
                RemoveCard(PlayerTurn, 6 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7))
                RemoveCard(PlayerTurn, 7 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8))
                RemoveCard(PlayerTurn, 8 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
                RemoveCard(PlayerTurn, 9 - 1);
            if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
            {
                if ((enemyAmount * 2) - playerCombatPoints > 0) //player didn't eliminate all enemies
                    PushBackPlayer(PlayerTurn, ((enemyAmount * 2) - playerCombatPoints + 1) / 2);

                if (PlayerTurn + 1 < players.Length)
                {
                    PlayerTurn++;
                    turnText.text = "Player's Turn: " + (PlayerTurn + 1);
                    playerCombatPoints = 0;

                    DoubleEnemiesForTruckRoute();
                }
                else
                {
                    enemyPhase = false;
                    phaseText.text = "Move Phase";
                    rollButton.interactable = true;
                    GameRound++;
                }
            }
        }
    }

    void ResetAllPossibleSpace()
    {
        //foreach (Transform child in transform)
        //{
        //    child.GetComponent<Space>().PossibleSpace = false;
        //}
        foreach (GameObject space in possibleSpaces)
        {
            space.GetComponent<Space>().PossibleSpace = false;
        }
        possibleSpaces.Clear();
    }

    void ShowPossibleSpaces(int playerNum, int moves)
    {
        ResetAllPossibleSpace();

        GoAlongPath(ps[playerNum].pathKey, ps[playerNum].spaceIndex, moves);

        foreach (GameObject possibleSpace in possibleSpaces)
        {
            possibleSpace.GetComponent<Space>().PossibleSpace = true;
        }
    }

    public bool IsSpecialSpace(string pathKey, int spaceIndex, GameObject[] specialSpaces)
    {
        Space space;
        foreach (GameObject specialSpaceObject in specialSpaces)
        {
            space = specialSpaceObject.gameObject.GetComponent<Space>();
            if (space.pathKey.Equals(pathKey) && space.spaceIndex == spaceIndex)
                return true;
        }
        return false;
    }
    int WhichQuest(string pathKey, int spaceIndex)
    {
        for (int i = 0; i < questStartSpaces.Length; i++)
        {
            if (questStartSpaces[i] == paths[pathKey][spaceIndex])
                return i;
        }
        for (int i = 0; i < questEndSpaces.Length; i++)
        {
            if (questEndSpaces[i] == paths[pathKey][spaceIndex])
                return i;
        }
        return -1;
    }

    void DoubleEnemiesForTruckRoute()
    {
        if (ps[PlayerTurn].pathKey.Equals(keyNames[1])) //if player is on truck route, double enemies
            enemyAmount *= 2;
        else
            enemyAmount = (GameRound + enemyIncrease - 1) / enemyIncrease;
        enemyAmountText.text = "Number Of Enemies: " + enemyAmount; //update text
    }

    public void ShowTextForSpace (string pathKey, int spaceIndex, bool active)
    {
        extraCardSpaceText.SetActive(active);
        if (active) //move text next to the space being hovered
            extraCardSpaceText.transform.position = Input.mousePosition + extraCardTextOffset;
    }

    void GoAlongPath(string pathKey, int spaceIndex, int movesLeft)
    {
        if (movesLeft >= 0 && !possibleSpaces.Contains(paths[pathKey][spaceIndex]))
        {
            possibleSpaces.Add(paths[pathKey][spaceIndex]);

            if (spaceIndex < FindEndOfPath(pathKey)) //forward
                GoAlongPath(pathKey, spaceIndex + 1, movesLeft - 1);
            else if (pathKey != keyNames[0]) //end of sidepath
            {
                foreach (Transform child in transform)
                {
                    Space childSpace = child.GetComponent<Space>();
                    if (childSpace.connectedPath1.Equals(pathKey) && !childSpace.startOfConnectedPath)
                        GoAlongPath(childSpace.pathKey, childSpace.spaceIndex, movesLeft - 1);
                    if (childSpace.connectedPath2.Equals(pathKey) && !childSpace.startOfConnectedPath)
                        GoAlongPath(childSpace.pathKey, childSpace.spaceIndex, movesLeft - 1);
                }
            }

            if (spaceIndex > 0) //backward
                GoAlongPath(pathKey, spaceIndex - 1, movesLeft - 1);
            else if (pathKey != keyNames[0]) //leaving from sidpath entry
            {
                foreach (Transform child in transform)
                {
                    Space childSpace = child.GetComponent<Space>();
                    if (childSpace.connectedPath1.Equals(pathKey) && childSpace.startOfConnectedPath)
                        GoAlongPath(childSpace.pathKey, childSpace.spaceIndex, movesLeft - 1);
                    if (childSpace.connectedPath2.Equals(pathKey) && childSpace.startOfConnectedPath)
                        GoAlongPath(childSpace.pathKey, childSpace.spaceIndex, movesLeft - 1);
                }
            }

            //side paths
            Space sideSpace = paths[pathKey][spaceIndex].GetComponent<Space>();
            if (sideSpace.connectedPath1 != "")
            {
                if (sideSpace.startOfConnectedPath) //starting on a sidepath
                    GoAlongPath(sideSpace.connectedPath1, 0, movesLeft - 1);
                else //going back to a sidepath
                    GoAlongPath(sideSpace.connectedPath1, FindEndOfPath(sideSpace.connectedPath1), movesLeft - 1);
            }
            if (sideSpace.connectedPath2 != "")
            {
                if (sideSpace.startOfConnectedPath)
                    GoAlongPath(sideSpace.connectedPath2, 0, movesLeft - 1);
                else
                    GoAlongPath(sideSpace.connectedPath2, FindEndOfPath(sideSpace.connectedPath2), movesLeft - 1);
            }
        }

    }

    int FindEndOfPath(string pathKey)
    {
        for (int i = 29; i > 0; i--)
        {
            if (paths[pathKey][i] != null)
            {
                return i; //last space of this path
            }
        }
        return -1; //path emtpy
    }

    public void MovePlayer(int playerNum, string pathKey, int spaceIndex)
    {
        players[playerNum].transform.position = paths[pathKey][spaceIndex].transform.position + playerOffsets[playerNum];
        ps[playerNum].pathKey = pathKey;
        ps[playerNum].spaceIndex = spaceIndex;

        if (IsSpecialSpace(pathKey, spaceIndex, extraCardSpaces))
            DrawCard(playerNum);
    }

    public void NextMoveTurn()
    {
        if (PlayerTurn + 1 < players.Length)
        {
            PlayerTurn++;
            ShowPossibleSpaces(PlayerTurn, rollMoves);
            turnText.text = "Player's Turn: " + (PlayerTurn + 1);
        }
        else
        {
            for (int i = 0; i < players.Length; i++)
            {
                DrawCard(i);
            }
            playerCombatPoints = 0;
            enemyPhase = true;
            phaseText.text = "Enemy Phase Enter 0 to Finish Turn";
            PlayerTurn = 0;
            turnText.text = "Player's Turn: " + (PlayerTurn + 1);
            rollText.text = "";

            ResetAllPossibleSpace();
            DoubleEnemiesForTruckRoute();
        }
    }

    void PushBackPlayer(int playerNum, int moves)
    {
        if (moves > 0)
        {
            if (ps[playerNum].spaceIndex - 1 >= 0)
                MovePlayer(playerNum, ps[playerNum].pathKey, ps[playerNum].spaceIndex - 1);
            else
            {
                foreach (Transform child in transform)
                {
                    Space childSpaceScript = child.GetComponent<Space>();
                    if (childSpaceScript.startOfConnectedPath)
                    {
                        if (childSpaceScript.connectedPath1.Equals(ps[playerNum].pathKey))
                            MovePlayer(playerNum, childSpaceScript.pathKey, childSpaceScript.spaceIndex);
                        if (childSpaceScript.connectedPath2.Equals(ps[playerNum].pathKey))
                            MovePlayer(playerNum, childSpaceScript.pathKey, childSpaceScript.spaceIndex);
                    }
                }
            }

            PushBackPlayer(playerNum, moves - 1);
        }
    }

    public int SetSpaceIndex(GameObject space) => spaceIndex++;
    public string SetPathKey() => keyNames[keyNameIndex];

    public void Roll()
    {
        rollMoves = Random.Range(1, 7);
        rollText.text = rollMoves.ToString();
        rollButton.interactable = false;

        PlayerTurn = 0;
        turnText.text = "Player's Turn: " + (PlayerTurn + 1);
        ShowPossibleSpaces(PlayerTurn, rollMoves);
    }

    void DrawCard(int playerNum)
    {
        ps[playerNum].cards.Add(Random.Range(1, 8));
        UpdatePlayerCards(playerNum);
    }

    void RemoveCard(int playerNum, int card)
    {
        playerCombatPoints += ps[playerNum].cards[card];
        ps[playerNum].cards.RemoveAt(card);
        UpdatePlayerCards(playerNum);
    }

    void UpdatePlayerCards(int playerNum)
    {
        string listOfCards = "";
        foreach (int card in ps[playerNum].cards)
            listOfCards += "|" + card;

        playerCardsText[playerNum].text = "Player " + (playerNum + 1) + ": " + listOfCards;
    }
}
