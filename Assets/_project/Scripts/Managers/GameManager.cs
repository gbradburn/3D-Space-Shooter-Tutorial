using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool ShouldQuitGame => Input.GetKeyUp(KeyCode.Escape);
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldQuitGame)
        {
            ShouldGame();
        }
    }

    void ShouldGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // todo handle WebGL
        Application.Quit();
#endif
    }
}
