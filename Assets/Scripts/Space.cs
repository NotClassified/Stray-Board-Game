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
                GetComponent<MeshRenderer>().material = mo.possibleSpaceMaterial;
            }
            else
            {
                GetComponent<MeshRenderer>().material = mo.impossibleSpaceMaterial;
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
        //for initializing space variables
        //if (pathKey == "" && Input.GetKey(KeyCode.LeftControl)) 
        //{
        //    //GetComponent<MeshRenderer>().material = mo.spaceMaterial;
        //    spaceIndex = mo.SetSpaceIndex(gameObject);
        //    pathKey = mo.SetPathKey();

        //    name = pathKey + spaceIndex;
        //}

        //show text for extra card space
        if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.extraCardSpaces)) 
        {
            mo.ShowTextForSpace(mo.extraCardSpaceText);
            textShowing = true;
        }
        //show text for quest start space
        else if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.questStartSpaces, false, "quest start")) 
        {
            mo.ShowTextForSpace(mo.questStartSpaceText);
            mo.ChangeTextForSpaceColor(pathKey, spaceIndex);
            textShowing = true;
        }
        //show text for quest end space
        else if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.questEndSpaces, false, "quest end")) 
        {
            mo.ShowTextForSpace(mo.questEndSpaceText);
            mo.ChangeTextForSpaceColor(pathKey, spaceIndex);
            textShowing = true;
        }
        //show text for Vending machine space
        else if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.vendingMachineSpaces, false, "vending machine")) 
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
            mo.MovePlayer(mo.PlayerTurn, pathKey, spaceIndex, true, false);
            mo.NextMoveTurn();
        }
    }
}
