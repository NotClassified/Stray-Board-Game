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

        if (mo.IsSpecialSpace(pathKey, spaceIndex, mo.extraCardSpaces))
        {
            mo.ShowTextForSpace(pathKey, spaceIndex, true); //show text
            textShowing = true;
        }
    }

    private void OnMouseExit()
    {
        if (textShowing)
        {
            mo.ShowTextForSpace(pathKey, spaceIndex, false);//unshow text
            textShowing = false;
        }
    }

    private void OnMouseDown()
    {
        if (PossibleSpace)
        {
            mo.MovePlayer(mo.PlayerTurn, pathKey, spaceIndex);
            mo.NextMoveTurn();
        }
    }
}
