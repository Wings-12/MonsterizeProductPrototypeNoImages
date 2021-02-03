using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Textゲームオブジェクトを取得するために必要

public class WorldCoordinateDisplay : MonoBehaviour
{
    // 変数設定
    Vector2 currentWorldCoordinate;

    // 知りたい座標のGameObjectの設定
    public GameObject target;

    // Update is called once per frame
    void Update()
    {
        currentWorldCoordinate = this.target.transform.position;

        float m_X = currentWorldCoordinate.x;
        float m_Y = currentWorldCoordinate.y;

        this.GetComponent<Text>().text = "キャラクター座標(" + currentWorldCoordinate.x.ToString("f1") + ", " + currentWorldCoordinate.y.ToString("f1") + ")";
    }
}
