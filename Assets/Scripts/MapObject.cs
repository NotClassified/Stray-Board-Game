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
    public Material vendinMachineMaterial;

    public Material[] playerMaterials;

    public string[] keyNames;
    public int keyNameIndex = 0;

    int spaceIndex = 0;
    public Dictionary<string, GameObject[]> paths;
    List<GameObject> possibleSpaces = new List<GameObject>();
    public Transform boardSpacesParent;

    public GameObject[] extraCardSpaces;
    public GameObject[] questStartSpaces;
    public GameObject[] questEndSpaces;
    public GameObject[] vendingMachineSpaces;
    public List<string> vendingMachineCards;
    string[] vendingMachineCardNames;

    public GameObject[] players;
    int playerTurn = -1;
    public int PlayerTurn
    {
        set
        {
            playerTurn = value;

            if (value >= 0 && value < players.Length)
            {
                turnText.color = playerMaterials[value].color;

                ps[value].PlayerTurn(true);
                //disable the previous player's turn state unless there is one player
                if (players.Length > 1 && value != 0)
                    ps[value - 1].PlayerTurn(false); //player before
            }
            if (value == -1)
                ps[players.Length - 1].PlayerTurn(false); //last player
        }
        get { return playerTurn; }
    }
    PlayerScript[] ps;
    public Vector3[] playerOffsets;
    public float playerScaleDuringTurn;
    public float playerScaleNotDuringTurn;

    int enemyAmount = 0;
    int enemiesLeft;
    int playerCombatPoints;
    int playerExtraCombatPoints;
    public int enemyIncrease = 2;
    public int enemyLimit;
    bool enemyPhase = false;
    int gameRound = 1;
    int GameRound
    {
        set
        {
            gameRound = value;
            SetEnemyAmount();
        }
        get { return gameRound; }
    }
    int rollMoves;

    public TextMeshProUGUI rollText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI enemyAmountText;
    public Button rollButton;
    public Button elevatorButton;
    public TextMeshProUGUI[] playerCardsText;
    public TextMeshProUGUI extraStatsText;

    public GameObject extraCardSpaceText;
    public GameObject questStartSpaceText;
    public GameObject questEndSpaceText;
    public GameObject vendingMachineSpaceText;
    public Vector3 specialSpaceTextOffset;

    public GameObject enemyPhaseScreenParent;
    public Button showEnemyPhaseScreenButton;
    public TextMeshProUGUI showEnemyPhaseScreenText;
    public TextMeshProUGUI enemyAmountEnemyPhaseText;
    public TextMeshProUGUI enemiesLeftText;

    public Transform playerCardsParent;
    List<int> selectedPlayerCards = new List<int>();
    public Color32 cardSelectedColor;
    public Color32 cardNotSelectedColor;

    void Awake()
    {
        //loading all pathways and space types
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
                else if (IsSpecialSpace(space.pathKey, space.spaceIndex, vendingMachineSpaces)) //quest end space
                    boardSpaceRenderer.material = vendinMachineMaterial;
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

        //get all the names of the vending machine cards
        vendingMachineCardNames = new string[vendingMachineCards.Count];
        for (int i = 0; i < vendingMachineCards.Count; i++)
        {
            vendingMachineCardNames[i] = vendingMachineCards[i];
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
        phaseText.text = "Roll Phase";
        turnText.text = "";
        rollButton.interactable = true;
        showEnemyPhaseScreenButton.interactable = false;
        if (IsEnemyPhaseScreenVisible())
            ToggleEnemyPhaseScreen();
    }

    private void Update()
    {
        //for initializing space variables
        //if (Input.GetKeyDown(KeyCode.N)) 
        //{
        //    spaceIndex = 0;
        //    keyNameIndex++;
        //}
    }

    #region SPACES
    void ResetAllPossibleSpace()
    {
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

    public void ShowTextForSpace (GameObject text)
    {
        text.SetActive(true); //activate text
        text.transform.position = Input.mousePosition + specialSpaceTextOffset; //move text next to mouse
    }
    public void UnShowTextForSpace() //deactivate all texts for spaces
    {
        extraCardSpaceText.SetActive(false);
        questStartSpaceText.SetActive(false);
        questEndSpaceText.SetActive(false);
        vendingMachineSpaceText.SetActive(false);
    }
    public void ChangeTextForSpaceColor(string pathKey, int spaceIndex)
    {
        int questNum = WhichQuest(pathKey, spaceIndex);
        if (questNum == -1) //prevent out of boundaries error
            return;

        if (IsSpecialSpace(pathKey, spaceIndex, questStartSpaces))
        {
            TextMeshProUGUI text = questStartSpaceText.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            Image image = questStartSpaceText.transform.GetChild(0).GetComponent<Image>();

            text.text = "Quest " + (questNum + 1) + " Start";
            //text.color = questStartSpaceMaterials[questNum].color;
            //make back image slightly transparent
            var tempColor = questStartSpaceMaterials[questNum].color;
            tempColor.a = .7f;
            image.color = tempColor;
        }
        else if (IsSpecialSpace(pathKey, spaceIndex, questEndSpaces))
        {
            TextMeshProUGUI text = questEndSpaceText.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            Image image = questEndSpaceText.transform.GetChild(0).GetComponent<Image>();

            text.text = "Quest " + (questNum + 1) + " End";
            //text.color = questEndSpaceMaterials[questNum].color;
            //make back image slightly transparent
            var tempColor = questEndSpaceMaterials[questNum].color;
            tempColor.a = .7f;
            image.color = tempColor;
        }
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
    #endregion

    public void MovePlayer(int playerNum, string pathKey, int spaceIndex, bool spaceChosenByPlayer, bool center)
    {
        Vector3 spacePos = paths[pathKey][spaceIndex].transform.position;
        if (center)
            players[playerNum].transform.position = spacePos;
        else
            players[playerNum].transform.position = spacePos + playerOffsets[playerNum];

        ps[playerNum].pathKey = pathKey;
        ps[playerNum].spaceIndex = spaceIndex;

        if (spaceChosenByPlayer) //if player is deciding to move to this space, check if the space is special
        {
            if (IsSpecialSpace(pathKey, spaceIndex, extraCardSpaces))
                DrawCard(playerNum);
            else if (IsSpecialSpace(pathKey, spaceIndex, vendingMachineSpaces))
                DrawVendingMachineCard(playerNum);
            //player starts quest if player hasn't started quest yet
            else if (IsSpecialSpace(pathKey, spaceIndex, questStartSpaces) 
                     && !HasQuestCard(playerNum, WhichQuest(pathKey, spaceIndex)))
                StartQuest(playerNum, WhichQuest(pathKey, spaceIndex));
            //player completes quest if player has started quest but hasn't completed quest yet
            else if (IsSpecialSpace(pathKey, spaceIndex, questEndSpaces)
                     && HasQuestCard(playerNum, WhichQuest(pathKey, spaceIndex))
                     && !HasCompletedQuestCard(playerNum, WhichQuest(pathKey, spaceIndex)))
                CompleteQuest(playerNum, WhichQuest(pathKey, spaceIndex));
        }
    }

    public void NextMoveTurn()
    {
        if (PlayerTurn + 1 < players.Length)
        {
            PlayerTurn++;
            turnText.text = "Player's Turn: " + (PlayerTurn + 1);

            //Bucket zipline (plus 1 movement minus 1 stealth)
            if (HasVendingMachineCard(PlayerTurn, vendingMachineCardNames[3])) 
                ShowPossibleSpaces(PlayerTurn, rollMoves + 1); 
            else
                ShowPossibleSpaces(PlayerTurn, rollMoves);
        }
        else
        {
            for (int i = 0; i < players.Length; i++)
            {
                DrawCard(i);
            }
            phaseText.text = "Stealth Phase";
            rollText.text = "";
            PlayerTurn = -1; //go back to first player
            NextEnemyPhaseTurn();

            ToggleEnemyPhaseScreen(); //show screen
            showEnemyPhaseScreenButton.interactable = true;
            UpdatePlayerCardsForEnemyPhase();

            ResetAllPossibleSpace();
        }
    }
    void NextEnemyPhaseTurn()
    {
        enemyPhase = true;
        if (PlayerTurn + 1 < players.Length)
        {
            PlayerTurn++;
            turnText.text = "Player's Turn: " + (PlayerTurn + 1);
            playerCombatPoints = 0;
            playerExtraCombatPoints = 0;
            selectedPlayerCards.Clear();

            //Activate Elevator Button if the player has the elevator card
            elevatorButton.interactable = HasVendingMachineCard(playerTurn, vendingMachineCardNames[3]);
            //give extra stats if the player has certain vending machine cards
            if (HasVendingMachineCard(playerTurn, vendingMachineCardNames[0])) //Robot carrying box
                playerExtraCombatPoints++;
            if (HasVendingMachineCard(playerTurn, vendingMachineCardNames[1])) //Barrel rolls through lasers
                playerExtraCombatPoints++;
            if (HasVendingMachineCard(playerTurn, vendingMachineCardNames[2])) //Bucket zipline
                playerExtraCombatPoints--;
            if (HasCompletedQuestCard(playerTurn, 1)) //What’s B-12 real name? (Quest 2)
                playerExtraCombatPoints += 4;

            if (playerExtraCombatPoints > 0)
                extraStatsText.text = "Extra Stealth Points: +" + playerExtraCombatPoints;
            else
                extraStatsText.text = "Extra Stealth Points: " + playerExtraCombatPoints;
            playerCombatPoints += playerExtraCombatPoints;

            SetEnemyAmount(); //make sure enemyPhase is true before this call
            UpdatePlayerCardsForEnemyPhase();
        }
        else
        {
            enemyPhase = false;
            phaseText.text = "Roll Phase";
            rollButton.interactable = true;
            turnText.text = "";

            if (IsEnemyPhaseScreenVisible())
                ToggleEnemyPhaseScreen(); //hide screen
            showEnemyPhaseScreenButton.interactable = false;

            PlayerTurn = -1;
            GameRound++; //make sure enemyPhase is false before this call
        }
    }
    public void FinishEnemyPhaseTurn() //calculate the amount of enemies the player passed
    {
        //Robot carrying box (plus 1 stealth stat)
        if (HasVendingMachineCard(PlayerTurn, vendingMachineCardNames[0]))
            playerCombatPoints += 1;
        //Barrel rolls through lasers (plus 1 stealth)
        if (HasVendingMachineCard(PlayerTurn, vendingMachineCardNames[1]))
            playerCombatPoints += 1;
        //Bucket zipline (plus 1 movement minus 1 stealth)
        if (HasVendingMachineCard(PlayerTurn, vendingMachineCardNames[2]))
            playerCombatPoints -= 1;

        while (selectedPlayerCards.Count > 0)
        {
            int maxIndex = 0;
            foreach (int cardIndex in selectedPlayerCards)
            {
                if (cardIndex > maxIndex)
                    maxIndex = cardIndex;
            }
            playerCombatPoints += ps[PlayerTurn].cards[maxIndex];
            ps[PlayerTurn].cards.RemoveAt(maxIndex);
            selectedPlayerCards.Remove(maxIndex);
        }

        //int enemyPointsLeft = (enemyAmount * 2) - playerCombatPoints;
        //if (enemyPointsLeft > 0) //player didn't eliminate all enemies
        //    PushBackPlayer(PlayerTurn, (enemyPointsLeft + 1) / 2);
        if (enemiesLeft > 0) //player didn't eliminate all enemies
            PushBackPlayer(PlayerTurn, enemiesLeft);

        NextEnemyPhaseTurn();
    }
    bool IsEnemyPhaseScreenVisible() => enemyPhaseScreenParent.activeSelf;
    public void ToggleEnemyPhaseScreen()
    {
        bool active = !enemyPhaseScreenParent.activeSelf;
        enemyPhaseScreenParent.SetActive(active);
        if (active)
            showEnemyPhaseScreenText.text = "Show Stealth\nPhase Screen";
        else
            showEnemyPhaseScreenText.text = "Hide Stealth\nPhase Screen";
    }
    void SetEnemyAmount()
    {
        int nextEnemyAmount = (GameRound + enemyIncrease - 1) / enemyIncrease;
        if (nextEnemyAmount <= enemyLimit)
            enemyAmount = nextEnemyAmount;
        if (enemyPhase) //prevent out of bounds exception
        {
            if (ps[PlayerTurn].pathKey.Equals(keyNames[1])) //if player is on truck route, double enemies
                enemyAmount *= 2;

            if (HasCompletedQuestCard(PlayerTurn, 0)) //if player has completed: Feeling cold (Quest 1)
                enemyAmount /= 2;
        }

        string enemyAndPoints = enemyAmount + " (" + enemyAmount * 2 + ")";
        enemyAmountText.text = "Number Of Enemies: " + enemyAndPoints;
        enemyAmountEnemyPhaseText.text = enemyAndPoints;
        enemiesLeftText.text = enemyAmount.ToString();
        enemiesLeft = enemyAmount;
    }

    void PushBackPlayer(int playerNum, int moves)
    {
        if (moves > 0)
        {
            if (ps[playerNum].spaceIndex - 1 >= 0)
                MovePlayer(playerNum, ps[playerNum].pathKey, ps[playerNum].spaceIndex - 1, false, false);
            else
            {
                foreach (Transform child in transform)
                {
                    Space childSpaceScript = child.GetComponent<Space>();
                    if (childSpaceScript.startOfConnectedPath)
                    {
                        if (childSpaceScript.connectedPath1.Equals(ps[playerNum].pathKey))
                            MovePlayer(playerNum, childSpaceScript.pathKey, childSpaceScript.spaceIndex, false, false);
                        if (childSpaceScript.connectedPath2.Equals(ps[playerNum].pathKey))
                            MovePlayer(playerNum, childSpaceScript.pathKey, childSpaceScript.spaceIndex, false, false);
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
        rollMoves = 6; print("rollMoves set to 6"); //for testing

        rollText.text = rollMoves.ToString();
        rollButton.interactable = false;
        phaseText.text = "Move Phase";

        PlayerTurn = -1;
        NextMoveTurn();
    }

    void DrawCard(int playerNum)
    {
        ps[playerNum].cards.Add(Random.Range(1, 8));
        //UpdatePlayerCards(playerNum);
    }
    void DrawVendingMachineCard(int playerNum)
    {
        if (vendingMachineCards.Count > 0)
        {
            string card = vendingMachineCards[Random.Range(0, vendingMachineCards.Count)];
            vendingMachineCards.Remove(card);
            ps[playerNum].vendingMachineCards.Add(card);

            if (vendingMachineCardNames[2].Equals(card)) //Bucket zipline
                playerCardsText[playerNum].text += "|+1 Move| ";
            if (vendingMachineCardNames[3].Equals(card)) //Elevator
                playerCardsText[playerNum].text += "|Elevator| ";
        }
    }
    bool HasVendingMachineCard(int playerNum, string card) => ps[playerNum].vendingMachineCards.Contains(card);
    void StartQuest(int playerNum, int questNum)
    {
        ps[playerNum].questCards.Add(questNum);
        playerCardsText[playerNum].text += "|Started Quest " + (questNum + 1) + "| ";
    }
    bool HasQuestCard(int playerNum, int card) => ps[playerNum].questCards.Contains(card);
    void CompleteQuest(int playerNum, int questNum)
    {
        ps[playerNum].questCardsComplete.Add(questNum);
        //delete |Started Quest #|
        int oldTextIndex = playerCardsText[playerNum].text.IndexOf("|Started Quest " + (questNum + 1) + "| ");
        playerCardsText[playerNum].text = playerCardsText[playerNum].text.Substring(0, oldTextIndex)
            + playerCardsText[playerNum].text.Substring(oldTextIndex + 18);
        //add new text: |Completed Quest #|
        playerCardsText[playerNum].text += "|Completed Quest " + (questNum + 1) + "| ";
    }
    bool HasCompletedQuestCard(int playerNum, int card) => ps[playerNum].questCardsComplete.Contains(card);

    void UpdatePlayerCardsForEnemyPhase()
    {
        
        int cardAmount = ps[PlayerTurn].cards.Count;
        int cardIndex = 0;
        for (int i = 0; i < 3; i++)
        {
            Transform row = playerCardsParent.GetChild(i);
            for (int j = 0; j < 3; j++)
            {
                Transform button = row.GetChild(j);
                button.GetComponent<Button>().image.color = cardNotSelectedColor;

                TextMeshProUGUI text = button.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (cardIndex < cardAmount)
                    text.text = ps[PlayerTurn].cards[cardIndex++].ToString();
                else
                    text.text = "";
            }
        }
    }
    public void SelectPlayerCard(int cardIndex)
    {
        if (cardIndex < ps[PlayerTurn].cards.Count) //prevent player from choosing a blank button
        {
            //get the button the player selected
            Button button = playerCardsParent.GetChild(cardIndex / 3).GetChild(cardIndex % 3).GetComponent<Button>();

            if (!selectedPlayerCards.Contains(cardIndex))
            {
                selectedPlayerCards.Add(cardIndex);
                //darken to show that this button is selected
                button.image.color = cardSelectedColor;

                playerCombatPoints += ps[PlayerTurn].cards[cardIndex];
            }
            else
            {
                selectedPlayerCards.Remove(cardIndex);
                //lighten to show that this button is deselected
                button.image.color = cardNotSelectedColor;

                playerCombatPoints -= ps[PlayerTurn].cards[cardIndex];
            }
            enemiesLeft = ((enemyAmount * 2) - playerCombatPoints + 1) / 2;
            if (enemiesLeft >= 0)
                enemiesLeftText.text = enemiesLeft.ToString();
            else
                enemiesLeftText.text = "0";
        }
    }
    public void UseElevatorCard()
    {
        if (HasVendingMachineCard(PlayerTurn, vendingMachineCardNames[3]))
            NextEnemyPhaseTurn();
    }
}
