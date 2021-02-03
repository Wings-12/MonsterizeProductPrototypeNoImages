using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Textゲームオブジェクトを取得するために必要

public class TouchPositionWorldCoordinateDisplay : MonoBehaviour
{
    //<summary>
    ///タッチ座標の座標
    ///</summary>
    Vector2 touchPosition;

    // Update is called once per frame
    void Update()
    {
        Update_touchPosition();
        this.GetComponent<Text>().text = "タッチ座標(" + touchPosition.x.ToString("f1") + ", " + touchPosition.y.ToString("f1") + ")";
    }

    /// <summary>
    /// 機能：キャラクターの移動先のタッチした座標をフレーム毎に更新する
    /// 
    /// 引数：なし
    /// 
    /// 戻り値：なし
    /// 
    /// 備考：参考サイト：忘れた。
    /// </summary>
    void Update_touchPosition()
    {
        if (Input.touchCount > 0)
        {
            // タッチオブジェクト
            Touch touch = Input.GetTouch(0);

            // -------------------------デバッグ中-------------------------//
            // TextUIを見つける
            GameObject textUI = GameObject.Find("touch.phase_Text");

            // textコンポーネントにtouch.phaseを表示する
            textUI.GetComponent<Text>().text = "タッチ状態は" + touch.phase;

            // -------------------------デバッグ中-------------------------//


            // タッチした座標を持つローカル変数
            this.touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        }
    }
}
