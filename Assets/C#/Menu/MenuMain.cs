using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuMain : MonoBehaviour
{
    public void SceneSwap(int Scene)
    {
        SceneManager.LoadScene(Scene);
    }
}
