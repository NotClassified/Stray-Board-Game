using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Sprite[] slides;
    static int slidesIndex = 0;
    [SerializeField] Image viewPort;

    void Start()
    {
        if (slidesIndex < 0 || slidesIndex >= slides.Length) //if out of bounds
            slidesIndex = 0;

        viewPort.sprite = slides[slidesIndex];
    }

    public void NextSlide()
    {
        //if it is the end of the tutorial then return to menu
        if (++slidesIndex < slides.Length)
        {
            viewPort.sprite = slides[slidesIndex];
        }
        else
            ReturnMenu();
    }
    public void PreviousSlide()
    {
        //if it is the first slide then return to menu
        if (--slidesIndex >= 0)
        {
            viewPort.sprite = slides[slidesIndex];
        }
        else
            ReturnMenu();
    }
    public void ReturnMenu()
    {
        SceneManager.LoadScene(0);
    }
}
