using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneResetter : MonoBehaviour
{
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}