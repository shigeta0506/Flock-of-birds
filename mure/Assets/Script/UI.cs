using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject resetButton; 
    [SerializeField] private GameObject quitButton;  

    public static bool isMenuActive = false;

     void Start()
    {
        //ç≈èââBÇ∑
        resetButton.SetActive(false);
        quitButton.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = !isMenuActive; 
            ToggleMenu(isMenuActive);
        }
    }

    void ToggleMenu(bool isActive)
    {
        //âüÇ≥ÇÍÇΩÇÁï\é¶
        resetButton.SetActive(isActive);
        quitButton.SetActive(isActive);
    }
}
