using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Textゲームオブジェクトを取得するために必要

public class BattleAreaMyRightSideDisplay : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Text>().text = "右 = " + BattleArea.myRightSide.ToString();
    }
}
