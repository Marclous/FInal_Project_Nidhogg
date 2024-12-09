using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理的命名空间

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 目标场景名称
    [SerializeField] private string triggeringTag = "Player"; // 触发切换的标签

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查触发器的碰撞对象是否有指定标签
        if (collision.CompareTag(triggeringTag))
        {
            // 切换到目标场景
            LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        // 检查是否设置了目标场景名称
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("未指定目标场景名称！");
        }
    }
}
