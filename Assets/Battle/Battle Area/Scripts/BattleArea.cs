using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 概要：Battle Areaのクラス
/// 詳細：①Battle Areaの座標　②Playerとタッチ間の距離
/// </summary>
/// <remarks>
/// </remarks>
public static class BattleArea
{
    /* 自エリアにおける各辺のワールド座標 */
    public const float myTopSide = 2.5f;
    public const float myBottomSide = -4.5f;
    public const float myLeftSide = -10.0f;
    public const float myRightSide = 0.0f;

    ///<summary>
    /// myRightSideよりも自エリア内側にある2番目のmyRightSide
    /// </summary>
    /// <remarks>
    /// 備考：Playerがこのy軸から右の自エリア内に入るとPlayerのx座標がmyRightSideに固定される。
    /// 理由：子供たちが遊んでくれた時に自エリア右でもスムーズに動くように感じてもらうため。
    /// </remarks>
    public static float mySecondRightSide;

    ///<summary>
    ///Playerとタッチ間の距離
    ///</summary>
    ///<remarks>
    ///用途：
    ///①Playerの指に隠れてPlayerが見えなくなることを防ぐ
    ///②mySecondRightSideを設定する
    ///</remarks>
    public static float distancePlayerAndTouch;

    /* 自エリアにおける各辺のローカル座標 */
    // 注意：現在は未使用
    //public const float myLocalTopSide = 233.0f;
    //public const float myLocalBottomSide = -417.0f;
    //public const float myLocalLeftSide = -774.0f;
    //public const float myLocalRightSide = -78.0f;

    /* 敵エリアにおける各辺の座標 */
    public const float enemyTopSide = myTopSide;
    public const float enemyBottomSide = myBottomSide;
    public const float enemyLeftSide = myRightSide;
    public const float enemyRightSide = - myLeftSide;

    /* 敵エリアにおける各辺のローカル座標 */
    // 注意：現在は未使用
    //public const float enemyLocalTopSide = myLocalTopSide;
    //public const float enemyLocalBottomSide = myLocalBottomSide;
    //public const float enemyLocalLeftSide = -(myLocalRightSide);
    //public const float enemyLocalRightSide = 954.0f;

    /// <summary>
    /// 機能：distancePlayerAndTouchにタッチ時の指と被らない幅を設定する。
    /// </summary>
    /// <remarks>
    /// 備考：参考サイト：
    /// ①＜ワールド座標とローカル座標の変換＞：https://dkrevel.com/unity-explain/space/
    /// ②【Unity】GameObjectの幅と高さを取得・変更する方法（RectTransform）：https://techno-monkey.hateblo.jp/entry/2018/05/12/150845
    /// ③RectTransformからワールド座標に変換する方法：http://alien-program.hatenablog.com/entry/2017/08/06/164258
    /// </remarks>
    static void Set_distancePlayerAndTouch()
    {
        // ワールド座標の幅を取得したいimageゲームオブジェクト(Player)を取得する
        GameObject player = GameObject.Find("Player");

        // PlayerのRectTransformを取得する
        RectTransform playerRectTransform = (RectTransform)player.transform;

        // Playerの幅(Width)を取得するローカル変数
        Vector2 playerWidth = Vector2.zero;

        // Playerの幅を取得する
        playerWidth = new Vector2(player.GetComponent<RectTransform>().sizeDelta.x, 0.0f);

        // 親のゲームオブジェクト(Canvas)
        GameObject canvas = GameObject.Find("Canvas");

        // ローカル座標からワールド座標に変換
        Vector2 temp_world_playerWidth = canvas.transform.TransformPoint(playerWidth);

        distancePlayerAndTouch = temp_world_playerWidth.x;
    }

    /// <summary>
    /// distancePlayerAndTouchを設定する
    /// </summary>
    /// <remarks>疑問：PlayerのWidthが小さいと、タッチ座標とPlayerが被ってPlayerが見えない場合があるがそれはどうするか？</remarks>
    static void Set_mySecondRightSide()
    {
        mySecondRightSide = BattleArea.myRightSide - distancePlayerAndTouch;
    }

    /// <summary>
    /// BattleAreaのコンストラクタ
    /// </summary>
    /// <remarks>
    /// 備考：BattleAreaクラス内で設定するために計算が必要なstatic変数の初期化をしている。
    /// </remarks>
    static BattleArea()
    {
        Set_distancePlayerAndTouch();
        Set_mySecondRightSide();
    }
}

