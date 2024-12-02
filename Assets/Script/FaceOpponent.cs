using UnityEngine;

public class SimpleFaceOpponent : PlayerMovement
{
    public string opponentTag;
    private Transform opponentJiarui;            // 当前对手的 Transform

    void Start(){

    }
    void Update()
    {

        if(playerTag == "Player 1"){
            opponentTag =  "Player 2";
        }
        if(playerTag == "Player 2"){
            opponentTag =  "Player 1";
        }


        // 检测场上是否有对手
        DetectOpponent();

        // 如果存在对手，则调整朝向
        if (opponent != null)
        {
            FaceOpponent();
        }
    }

    // 检测场上是否存在对手
    void DetectOpponent()
    {
        GameObject opponentObject = GameObject.FindWithTag(opponentTag); // 查找带有指定标签的对象
        if (opponentObject != null)
        {
            opponent = opponentObject.transform; // 如果找到对手，则更新引用
        }
        else
        {
            opponent = null; // 如果没有找到对手，则清空引用
        }
    }

    // 面向对手
    void FaceOpponent()
    {
        Vector3 direction = (opponent.position - transform.position).normalized;

        // 对于 2D 游戏，翻转玩家朝向
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 面向左侧
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1); // 面向右侧
        }

        // 对于 3D 游戏，使用旋转朝向对手
        // Uncomment the following if working in 3D
        /*
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        */
    }
}
