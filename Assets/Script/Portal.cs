using UnityEngine;
using UnityEngine.SceneManagement; // Required to manage scenes

public class Portal : MonoBehaviour
{
    public string targetSceneName; // Name of the scene to load

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player 1"))
    {
        LoadTargetScene();
    }
}

    private void LoadTargetScene()
    {
        // Check if the target scene name is valid
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target scene name is not set in the Portal script.");
        }
    }
}
