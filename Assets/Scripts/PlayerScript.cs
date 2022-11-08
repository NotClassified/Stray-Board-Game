using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    public int spaceIndex;
    public string pathKey = "";
    public int playerNum;
    public bool area2;

    public MapObject mo;
    Outline outline;

    public List<int> cards;
    public List<string> vendingMachineCards;
    public List<int> questCards;
    public List<int> questCardsComplete;

    public bool skipEnemyPhase = false;


    private IEnumerator Start()
    {
        outline = GetComponent<Outline>();

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        spaceIndex = 0;
        pathKey = mo.keyNames[0];
        cards = new List<int>();

        GetComponent<MeshRenderer>().material = mo.playerMaterials[playerNum];
    }

    void ChangePlayerScale(float newScale) => transform.localScale = new Vector3(newScale, newScale, 1);

    public void PlayerTurn(bool isPlayerTurn)
    {
        if (isPlayerTurn) //this player's turn
        {
            outline.enabled = true;
            mo.MovePlayer(playerNum, pathKey, spaceIndex, false, true); //place player in the center
            transform.position -= Vector3.forward; //place player in front of other players
            ChangePlayerScale(mo.playerScaleDuringTurn); //make player bigger
        }
        else //not this player's turn
        {
            outline.enabled = false; 
            mo.MovePlayer(playerNum, pathKey, spaceIndex, false, false); //place player offset
            ChangePlayerScale(mo.playerScaleNotDuringTurn); //return player back to normal scale
        }
    }
}
