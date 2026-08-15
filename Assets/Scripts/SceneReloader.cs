using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour {
    
    public void Reload()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentLevel);
    }
    
}
