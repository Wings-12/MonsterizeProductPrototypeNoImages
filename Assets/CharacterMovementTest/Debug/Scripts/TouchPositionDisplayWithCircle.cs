using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchPositionDisplayWithCircle : MonoBehaviour
{
    //<summary>
    ///タッチ座標の座標
    ///</summary>
    Vector2 touchPosition;

    //<summary>
    ///Imageコンポーネント
    ///</summary>
    Image image;

    //<summary>
    ///Imageコンポーネントのα値(透明度)
    ///</summary>
    float alfa;

    //<summary>
    ///RGBを操作するための変数
    ///</summary>
    float red, green, blue;

    // Start is called before the first frame update
    void Start()
    {
        // 初期化
        touchPosition = Vector2.zero;

        // このゲームオブジェクトのImageコンポーネントを取得する
        this.image = this.gameObject.GetComponent<Image>();

        // このゲームオブジェクトのα値を取得する
        this.alfa = this.image.color.a;

        // このゲームオブジェクトのRGBを取得する
        this.red = this.image.color.r;
        this.green = this.image.color.g;
        this.blue = this.image.color.b;

        // このゲームオブジェクトを透明にする
        this.image.color = new Color(this.red, this.green, this.blue, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Update_touchPosition();

        // タッチされている場合
        if (Input.touchCount > 0)
        {
            // このゲームオブジェクトの透明状態を解除する
            this.image.color = new Color(this.red, this.green, this.blue, this.alfa);
        }

        // タッチされていない場合
        if (Input.touchCount == 0)
        {
            // このゲームオブジェクトを透明にする
            this.image.color = new Color(this.red, this.green, this.blue, 0.0f);
        }
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

            // タッチした座標を持つローカル変数
            this.touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            // タッチ座標をこのゲームオブジェクトの座標に設定する
            this.transform.position = this.touchPosition;
        }
    }
}
