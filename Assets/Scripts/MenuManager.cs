using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI playerCountText;

    private void Start()
    {
        SetCountText();
    }

    public void ChangeScene(int buildIndex) => SceneManager.LoadScene(buildIndex);

    public void AddPlayerCount(int add) => SetPlayerCount(PlayerCount.count + add);
    public void SetPlayerCount(int val)
    {
        if (val > 0 && val <= 4)
        {
            PlayerCount.count = val;
            SetCountText();
        }
    }

    void SetCountText() => playerCountText.text = PlayerCount.count.ToString();
}
