using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    public int spaceIndex;
    public string pathKey = "";
    public int playerNum;

    public MapObject mo;
    Outline outline;

    public List<int> cards;

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

    private void FixedUpdate()
    {
        if (!outline.enabled && mo.PlayerTurn == playerNum) //this player's turn
            outline.enabled = true;
        else if (outline.enabled && mo.PlayerTurn != playerNum) //not this player's turn
            outline.enabled = false;
    }
}
