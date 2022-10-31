using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Space : MonoBehaviour
{
    public MapObject mo;

    bool textShowing;
    private bool possibleSpace;
    public bool PossibleSpace
    {
        set
        {
            possibleSpace = value;

            if (value)
            {
                //GetComponent<MeshRenderer>().material = mo.playerMaterial[mo.PlayerTurn];
                GetComponent<MeshRenderer>().material = mo.possibleSpaceMaterial;
                //gameObject.SetActive(true);
            }
            else
            {
                GetComponent<MeshRenderer>().material = mo.impossibleSpaceMaterial;
                //gameObject.SetActive(false);
            }
        }
        get
        {
            return possibleSpace;
        }
    }

    public int spaceIndex;
    public string pathKey = "";

    public string connectedPath1;
    public string connectedPath2;
    public bool startOfConnectedPath;

    private void OnMouseEnter()
    {
        if (pathKey == "" && Input.GetKey(KeyCode.LeftControl)) //for initializing space variables
        {
            //GetComponent<MeshRenderer>().material = mo.spaceMaterial;
            spaceIndex = mo.SetSpaceIndex(gameObject);
            pathKey = mo.SetPathKey();

            name = pathKey + spaceIndex;
        }

        if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.extraCardSpaces)) //show text for extra card space
        {
            mo.ShowTextForSpace(mo.extraCardSpaceText);
            textShowing = true;
        }
        else if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.questStartSpaces)) //show text for quest start space
        {
            mo.ShowTextForSpace(mo.questStartSpaceText);
            mo.ChangeTextForSpaceColor(pathKey, spaceIndex);
            textShowing = true;
        }
        else if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.questEndSpaces)) //show text for quest end space
        {
            mo.ShowTextForSpace(mo.questEndSpaceText);
            mo.ChangeTextForSpaceColor(pathKey, spaceIndex);
            textShowing = true;
        }
        else if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.vendingMachineSpaces)) //show text for Vending machine space
        {
            mo.ShowTextForSpace(mo.vendingMachineSpaceText);
            textShowing = true;
        }
    }

    private void OnMouseExit()
    {
        if (textShowing)
        {
            mo.UnShowTextForSpace();
            textShowing = false;
        }
    }

    private void OnMouseDown()
    {
        if (PossibleSpace)
        {
            mo.MovePlayer(mo.PlayerTurn, pathKey, spaceIndex, false, false);
            mo.NextMoveTurn();
        }
    }
}
