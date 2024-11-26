using UnityEngine;

public class End : MonoBehaviour
{
    //ÉfÉÇÇÃèIóπ
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
