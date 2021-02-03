using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // デバッグのため追加 2020/09/08

// ///<summary>
///備考：青エリアの移動処理の概略：
///以下のように青エリアは9つの処理で構成されている。
///
///－－－－－－－－－－－－－－－－－－－－ －－－－－－－－－－－－－－－－－－－－ | 
/// 画面|            左側背景               |               右側背景                 |
/// 外列|                                   |                                        |
/// ↓   －－－－－－－－－－－－－－－－－ |                                        | 
/// 9.外|        3.外上青エリア        |4.外|                                        |
/// 左上| ※下端を含み左右端は含まない |右上|                                        |
/// －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－ |
/// 7.  |                              |2.  |                                        |
/// 外　|                              |右  |                                        |
/// 左　|                              |青  |                                        |
/// 青　|    1.メイン青エリア          |エ  |               赤エリア                 |
/// エ　|    ※1：端を含まない         |リ  |※赤エリアは左端(myRightSide)を含み、   |
/// リ　|    ※2：2.と7.が左右端を含む |ア  |また、青右関係エリア(2.と4.と6.)も      |
/// ア  |                              |    |myRightSideを含む。                     |
///   　|                              |    |                                        |
///   　|                              |    |                                        |
/// －－－－－－－－－－－－－－－－－－－－|－－－－－－－－－－－－－－－－－－－－|
/// 8.外|       5.外下青エリア         |6.外|               画面外                   
/// 左下| ※上端を含み左右端は含まない |右下|                                        
/// －－－－－－－－－－－－－－－－－－－－|
/// 
/// 前提：Playerは常にタッチ座標よりも指に隠れない程度に右側に描画されている。(このことを、「タッチした座標より少し右」と以下で表現する。)
/// 1.メイン青エリア：タッチした座標より少し右にPlayerが移動する。
/// 
/// 2.右青エリア：
/// ①1青右エリアの右端からPlayerの幅の半分の座標間をタッチした場合に、Playerのx座標は固定される。
/// ②①以外の青右エリアをタッチした場合に、Playerとタッチ座標は、青右エリアの右端側をタッチすればするほど、タッチ座標がPlayerの座標へ縮まっていく。
/// 
/// 3.上青エリア：
/// ①上青エリア内をタッチした場合は、タッチした座標より少し右にPlayerが移動する。
/// ②外上青エリアをタッチした場合は、Playerが上青エリア内の、タッチした外上青エリアに一番近い座標に移動する。
/// 
/// 4.右上(エリア)：
/// 
/// 
/// ステータス：設計中。
///</summary

/// <summary>
/// 画面上の青エリア内のみPlayerをタッチ操作で移動できるクラス
/// </summary>
public class PlayerMovements : BaseCharactersMovementByTouch
{
    #region フィールド
    ///<summary>
    ///Playerの初期座標
    ///</summary>
    readonly Vector2 iniPlayerPosition = new Vector2(-6.0f, -1.3f);

    #endregion

    #region メソッド

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        this.transform.position = iniPlayerPosition;

        // タッチ座標もiniPlayerPositionで初期化して、タッチされるまで、スタート地点からPlayerが動かないようにする
        //base.touchPosition = iniPlayerPosition;

        // タッチ座標は画面左側ではない、テキトーな座標で設定して、ゲーム開始時にスタート地点からPlayerが動かないようにする
        // 詳細：MovePlayerのローカル変数であるmoveDestinationが画面右側をタッチすると設定され、その際にPlayerが動くので、
        // 画面右側ではない座標を設定して、画面左側をタッチするまではPlayerが動かないようにしている。
        base.touchPosition = new Vector2(9999.0f, 9999.0f);
    }

    // Update is called once per frame
    // 文法：newについて
    // newはvirtualを使わなくても派生クラスでオーバーライドできる。
    // また、呼び出しの際は、宣言した変数の型によって派生クラスのインスタンスが呼ばれるか、基底クラスのインスタンスが呼ばれるかが決まる。
    // 参考URL：https://araramistudio.jimdo.com/2019/02/08/c-%E3%81%A7%E5%9F%BA%E5%BA%95%E3%82%AF%E3%83%A9%E3%82%B9%E3%81%AE%E3%83%A1%E3%82%BD%E3%83%83%E3%83%89%E3%82%92%E7%BD%AE%E3%81%8D%E6%8F%9B%E3%81%88%E3%82%8B%E3%82%AA%E3%83%BC%E3%83%90%E3%83%BC%E3%83%A9%E3%82%A4%E3%83%89/
    new void Update()
    {
        // 画面左側をタッチした場合のみタッチした座標を更新する
        // 注意：修正要。理由：画面左側ではなく、青エリア内、もしくは青エリア上端に隣接している矩形領域をタッチした場合に更新するため。
        this.Update_touchPositionIfTouchedOnBlueArea();

#if DEBAG
            // どんなデバッグか？：画面右側をタッチした後にPlayerが少し右に動いてしまうバグの原因を確認するためのデバッグ。
            if (Input.touchCount > 0)
            {
                Debug.Log("");
            }
#endif

        // Playerの色を変えてデバッグ (自エリア外に出たら黄色)
        this.GetComponent<Image>().color = Color.yellow;

        // メイン青エリア内をタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        // Playerをタッチよりも右側に移動する理由：Playerの指でPlayer(キャラクター)が隠れないようにするため
        if (
            (BattleArea.myLeftSide < base.touchPosition.x && base.touchPosition.x < BattleArea.mySecondRightSide)
            &&
            (BattleArea.myTopSide > base.touchPosition.y && base.touchPosition.y > BattleArea.myBottomSide)
            )
        {
            MovePlayerInMainBlueArea();
        }

        // 右青エリアをタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        // 注意：↑は厳密にはちょっと違う。詳細はMovePlayerInRightBlueAreaメソッドを参照
        else if (
            (BattleArea.mySecondRightSide <= base.touchPosition.x && base.touchPosition.x <= BattleArea.myRightSide)
            &&
             (BattleArea.myBottomSide < base.touchPosition.y && base.touchPosition.y < BattleArea.myTopSide)
             )
        {
            MovePlayerInRightBlueArea();
        }

        //// 外上青エリアをタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        //// 注意：↑は厳密にはちょっと違う。詳細はMovePlayerInOutsideTopBlueAreaメソッドを参照
        //else if (
        //    BattleArea.myTopSide <= base.touchPosition.y
        //    &&
        //     (BattleArea.myLeftSide < base.touchPosition.x && base.touchPosition.x < BattleArea.mySecondRightSide)
        //     )
        //{
        //    MovePlayerInOutsideTopBlueArea();

            
        //}

        //// 外右上青エリアをタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        //// 注意：↑は厳密にはMovePlayerInOutsideTopRightBlueAreaメソッドを参照
        //else if (
        //    BattleArea.myTopSide <= base.touchPosition.y
        //    &&
        //     (BattleArea.mySecondRightSide <= base.touchPosition.x && base.touchPosition.x <= BattleArea.myRightSide)
        //     )
        //{
        //    MovePlayerInOutsideTopBlueRightArea();
        //}

        // 外下青エリアをタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        // 注意：↑は厳密にはMovePlayerInOutsideTopRightBlueAreaメソッドを参照
        else if (
            base.touchPosition.y <= BattleArea.myBottomSide
            &&
             (BattleArea.myLeftSide < base.touchPosition.x && base.touchPosition.x < BattleArea.mySecondRightSide)
             )
        {
            MovePlayerInOutsideBottomBlueArea();
        }

        // ///<summary>
        ///備考：青エリアの移動処理の概略：
        ///以下のように青エリアは9つの処理で構成されている。
        ///－－－－－－－－－－－－－－－－－－－－ －－－－－－－－－－－－－－－－－－－－ | 
        /// 画面|            左側背景               |               右側背景                 |
        /// 外列|                                   |                                        |
        /// ↓   －－－－－－－－－－－－－－－－－ |                                        | 
        /// 9.外|        3.外上青エリア        |4.外|                                        |
        /// 左上| 理由：下端を含み左右端は含まない |右上|                                        |
        /// －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－ |
        /// 7.  |                              |2.  |                                        |
        /// 外　|                              |右  |                                        |
        /// 左　|                              |青  |                                        |
        /// 青　|    1.メイン青エリア          |エ  |               赤エリア                 |
        /// エ　|    ※1：端を含まない         |リ  |※赤エリアは左端(myRightSide)を含み、   |
        /// リ　|    ※2：2.と7.が左右端を含む |ア  |また、青右関係エリア(2.と4.と6.)も      |
        /// ア  |                              |    |myRightSideを含む。                     |
        ///   　|                              |    |                                        |
        ///   　|                              |    |                                        |
        /// －－－－－－－－－－－－－－－－－－－－|－－－－－－－－－－－－－－－－－－－－|
        /// 8.外|       5.外下青エリア         |6.外|               画面外                   
        /// 左下| ※上端を含み左右端は含まない |右下|                                        
        /// －－－－－－－－－－－－－－－－－－－－|

        // 赤エリアをタッチした場合にPlayerを止める
        // 備考：バグ対策(右青エリアから赤エリア中央へフリック入力すると、キャラクターがフリック開始座標付近の青右エリア右端辺りに戻ってきてしまうバグ)
        // →Update_touchPositionIfTouchedOnLeftScreenの実装により、画面右側のタッチ処理をしなくなったので、この処理がなくてもバグらなくなったと思われる(簡単にテストした。)。
        // →ただ、もしかしたらバグが残っているかもしれないので、念のため残しておく。
        //else if (base.touchPosition.x > BattleArea.myRightSide)
        //{
        //    base.rigidbody2D.velocity = Vector2.zero;
        //}

        // Playerが青エリアから出たら、一番近い自エリア内に戻す
        //Return_playerPositionToClosestPositionInBlueArea();
    }

    /// <summary>
    /// 機能：外下青エリア内をタッチした場合にPlayerをスマホタッチ操作で移動する
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：　
    /// ①このメソッドは、呼び出し元の条件文である外下青エリア内をタッチした場合のみ使用できる。
    /// ②青エリアの下端以下をタッチした場合にPlayerのy座標は青エリア下端からPlayerの高さの半分の座標になる。
    /// </summary>
    void MovePlayerInOutsideBottomBlueArea()
    {
        // Playerの色を変えてデバッグ (外下青エリア内をタッチした場合に白色と黒色で点滅する)
        this.GetComponent<Image>().color = Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time, 1.0f));

        // // Playerのy座標を青エリア下端に固定するローカル変数の初期化
        // 用途：外下青エリアをタッチしてもPlayerをスムーズに動かすために、以下の条件文でこの値をthis.transform.positionに設定するために使う。
        Vector2 moveDestinationFor_myBottomSide = new Vector2(base.touchPosition.x + BattleArea.distancePlayerAndTouch, BattleArea.myBottomSide);

        // --------------------------------------以下未修正------------------------------------------------
        float standardValueToStopPlayer = 0.5f;

        // 移動先がPlayerより右側かつ、Playerが青エリア下端以下にいた場合
        if (
            (this.transform.position.x < moveDestinationFor_myBottomSide.x)
            &&
            (this.transform.position.y <= BattleArea.myBottomSide)
            )
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に左右にぶれて動かないように追加した処理
            if (this.transform.position.x >= moveDestinationFor_myBottomSide.x - standardValueToStopPlayer)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = moveDestinationFor_myBottomSide;
            }
            else if (this.transform.position.x < moveDestinationFor_myBottomSide.x - standardValueToStopPlayer)
            {
                // Playerが右に動く
                base.rigidbody2D.velocity = new Vector2(base.characterSpeed, 0.0f);
            }
        }
        // 移動先がPlayerより左側かつ、Playerが青エリア下端以下にいた場合
        else if (
            (base.touchPosition.x <= moveDestinationFor_myBottomSide.x)
            &&
            (this.transform.position.y <= BattleArea.myBottomSide)
            )
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に左右にぶれて動かないように追加した処理
            if (moveDestinationFor_myBottomSide.x + standardValueToStopPlayer >= this.transform.position.x)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = moveDestinationFor_myBottomSide;
            }
            else if (moveDestinationFor_myBottomSide.x + standardValueToStopPlayer < this.transform.position.x)
            {
                // Playerが左に動く
                base.rigidbody2D.velocity = new Vector2(-base.characterSpeed, 0.0f);
            }
        }
        // Playerが青エリア下端より上にいた場合
        else if (this.transform.position.y > BattleArea.myBottomSide)
        {
            this.MovePlayer();
        }

        // --------------------------------------以上未修正------------------------------------------------
    }

    /// <summary>
    /// 機能：外右上青エリア内をタッチした場合にPlayerをスマホタッチ操作で移動する
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：　
    /// ①このメソッドは、呼び出し元の条件文である外右上青エリア内をタッチした場合のみ使用できる。
    /// ②外右上青エリアの下端以上をタッチした場合にPlayerのx座標は青エリア右端、y座標は青エリア上端に固定される。
    /// ③MovePlayerInRightBlueAreaと同じく、Playerとタッチ座標は、右青エリアの右端側をタッチすればするほど、タッチ座標がPlayerの座標へ縮まっていく。
    /// </summary>
    void MovePlayerInOutsideTopBlueRightArea()
    {
        // Playerの色を変えてデバッグ (外上青エリア内をタッチした場合に灰色)
        this.GetComponent<Image>().color = Color.gray;

        // Playerのx座標は青エリア右端、y座標は青エリア上端に固定するローカル変数の初期化
        Vector2 moveDestinationFor_myToprRightSide = new Vector2(BattleArea.myRightSide, BattleArea.myTopSide);

        float standardValueToStopPlayer = 0.5f;

        // 移動先がPlayerより右側かつ、Playerが青エリア上端以上にいた場合
        if (
            (this.transform.position.x < moveDestinationFor_myToprRightSide.x)
            &&
            (this.transform.position.y >= BattleArea.myTopSide)
            )
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に左右にぶれて動かないように追加した処理
            if (this.transform.position.x >= moveDestinationFor_myToprRightSide.x - standardValueToStopPlayer)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = moveDestinationFor_myToprRightSide;
            }
            else if (this.transform.position.x < moveDestinationFor_myToprRightSide.x - standardValueToStopPlayer)
            {
                // Playerが右に動く
                base.rigidbody2D.velocity = new Vector2(base.characterSpeed, 0.0f);
            }
        }
        // 移動先がPlayerより左側かつ、Playerが青エリア上端以上にいた場合
        else if (
            (base.touchPosition.x <= moveDestinationFor_myToprRightSide.x)
            &&
            (this.transform.position.y >= BattleArea.myTopSide)
            )
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に左右にぶれて動かないように追加した処理
            if (moveDestinationFor_myToprRightSide.x + standardValueToStopPlayer >= this.transform.position.x)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = moveDestinationFor_myToprRightSide;
            }
            else if (moveDestinationFor_myToprRightSide.x + standardValueToStopPlayer < this.transform.position.x)
            {
                // Playerが左に動く
                base.rigidbody2D.velocity = new Vector2(-base.characterSpeed, 0.0f);
            }
        }
        // Playerが青エリア上端より下にいた場合
        else if (this.transform.position.y < BattleArea.myTopSide)
        {
            this.MovePlayer();
        }
    }

    /// <summary>
    /// 機能：外上青エリア内をタッチした場合にPlayerをスマホタッチ操作で移動する
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：　
    /// ①このメソッドは、呼び出し元の条件文である外上青エリア内をタッチした場合のみ使用できる。
    /// ②青エリアの上端以上をタッチした場合にPlayerのy座標は青エリア上端からPlayerの高さの半分の座標になる。
    /// </summary>
    void MovePlayerInOutsideTopBlueArea()
    {
        // Playerの色を変えてデバッグ (外上青エリア内をタッチした場合に緑)
        this.GetComponent<Image>().color = Color.green;

        // Playerのy座標を青エリア上端に固定するローカル変数の初期化
        // 用途：外上青エリアをタッチしてもPlayerをスムーズに動かすために、以下の条件文でこの値をthis.transform.positionに設定するために使う。
        Vector2 moveDestinationFor_myTopSide = new Vector2(base.touchPosition.x + BattleArea.distancePlayerAndTouch, BattleArea.myTopSide);

        float standardValueToStopPlayer = 0.5f;

        // 移動先がPlayerより右側かつ、Playerが青エリア上端以上にいた場合
        if (
            (this.transform.position.x < moveDestinationFor_myTopSide.x)
            &&
            (this.transform.position.y >= BattleArea.myTopSide)
            )
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に左右にぶれて動かないように追加した処理
            if (this.transform.position.x >= moveDestinationFor_myTopSide.x - standardValueToStopPlayer)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = moveDestinationFor_myTopSide;
            }
            else if (this.transform.position.x < moveDestinationFor_myTopSide.x - standardValueToStopPlayer)
            {
                // Playerが右に動く
                base.rigidbody2D.velocity = new Vector2(base.characterSpeed, 0.0f);
            }
        }
        // 移動先がPlayerより左側かつ、Playerが青エリア上端以上にいた場合
        else if (
            (base.touchPosition.x <= moveDestinationFor_myTopSide.x)
            &&
            (this.transform.position.y >= BattleArea.myTopSide)
            )
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に左右にぶれて動かないように追加した処理
            if (moveDestinationFor_myTopSide.x + standardValueToStopPlayer >= this.transform.position.x)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = moveDestinationFor_myTopSide;
            }
            else if (moveDestinationFor_myTopSide.x + standardValueToStopPlayer < this.transform.position.x)
            {
                Debug.Log("青上右移動");
                // Playerが左に動く
                base.rigidbody2D.velocity = new Vector2(-base.characterSpeed, 0.0f);
            }
        }
        // Playerが青エリア上端より下にいた場合
        else if(this.transform.position.y < BattleArea.myTopSide)
        {
            this.MovePlayer();
        }
    }

    /// <summary>
    /// 機能：右青エリア内をタッチした場合にPlayerをスマホタッチ操作で移動する
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：　
    /// ①このメソッドは、呼び出し元の条件文である右青エリア内をタッチした場合のみ使用できる。
    /// ②右青エリア内にPlayerが入った場合の移動は大きく分けて以下の2通りある。
    /// 1. 右青エリアの右端からPlayerの幅の半分の座標間をタッチした場合に、Playerのx座標は固定される。
    /// 2. 1.以外の右青エリアをタッチした場合に、Playerとタッチ座標は、右青エリアの右端側をタッチすればするほど、タッチ座標がPlayerの座標へ縮まっていく。
    /// </summary>
    void MovePlayerInRightBlueArea()
    {
        // Playerの色を変えてデバッグ (右青エリア内にいたらマゼンタ)
        this.GetComponent<Image>().color = Color.magenta;

        // Playerのx座標を固定して、y座標を座標に設定するローカル変数の初期化
        // 用途：Playerに不快感を与えないようにタッチ座標にPlayerを描画するために、以下の条件文でこの値をthis.transform.positionに設定するために使う。
        Vector2 playerPositionOnlyXCoordinateFixed = new Vector2(BattleArea.myRightSide, this.transform.position.y);

        float standardValueToStopPlayer = 0.5f;

        // Playerより上側をタッチした場合
        if (playerPositionOnlyXCoordinateFixed.y < base.touchPosition.y)
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に上下にぶれて動かないように追加した処理
            if (playerPositionOnlyXCoordinateFixed.y >= base.touchPosition.y - standardValueToStopPlayer)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = new Vector2(playerPositionOnlyXCoordinateFixed.x, base.touchPosition.y);
            }
            else if (base.touchPosition.y - standardValueToStopPlayer > playerPositionOnlyXCoordinateFixed.y)
            {
                // Playerが上に動く
                base.rigidbody2D.velocity = new Vector2(0.0f, base.characterSpeed);
            }
        }

        //Playerより下側の座標をタッチした場合
        else if (base.touchPosition.y <= playerPositionOnlyXCoordinateFixed.y)
        {
            // PlayerがtouchPositionに近づいた場合
            // 備考：PlayerがtouchPositionに来た時に上下にぶれて動かないように追加した処理
            if (base.touchPosition.y + standardValueToStopPlayer >= playerPositionOnlyXCoordinateFixed.y)
            {
                // Playerが止まる
                base.rigidbody2D.velocity = Vector2.zero;

                // タッチ座標にPlayerをワープして移動する
                this.transform.position = new Vector2(playerPositionOnlyXCoordinateFixed.x, base.touchPosition.y);
            }
            else if (playerPositionOnlyXCoordinateFixed.y > base.touchPosition.y + standardValueToStopPlayer)
            {
                // Playerが下に動く
                base.rigidbody2D.velocity = new Vector2(0.0f, -base.characterSpeed);
            }
        }
    }

    /// <summary>
    /// 機能：メイン青エリア内をタッチした場合にPlayerをスマホタッチ操作で移動する
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：このメソッドは、呼び出し元の条件文であるメイン青エリア内をタッチした場合のみ使用できる。
    /// </summary>
    void MovePlayerInMainBlueArea()
    {
        // Playerの色を変えてデバッグ (自エリア内にいたら白)
        this.GetComponent<Image>().color = Color.white;

        this.MovePlayer();

        //Debug.Log("エリア内にいる場合のPlayerの座標は" + this.transform.position);
    }

    /// <summary>
    /// 機能：Playerが青エリアから出たら、一番近い自エリア内に戻す
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：青エリア上端の処理しか作ってないので、他の端を出た場合の設計をする
    /// </summary>
    void Return_playerPositionToClosestPositionInBlueArea()
    {
        // Playerが青エリア上端の外側を出た場合に、Playerを青エリア上端に戻す
        if (this.transform.position.y > BattleArea.myTopSide)
        {
            this.transform.position = new Vector2(this.transform.position.x, BattleArea.myTopSide);

            // Playerが移動しないようにタッチ座標もPlayerが戻ってくる座標で更新する
            base.touchPosition = this.transform.position;
        }

       
    }

    /// <summary>
    /// 機能：自エリア内でPlayerをスマホタッチ操作で移動する。
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：このメソッドでPlayerの移動座標をタッチ座標の指等で隠れない座標に更新しているので注意。
    /// ※本当はこの移動座標更新処理はこのメソッドから切り分けたほうが良いと思うが、まだリファクタリングできていない。
    /// </summary>
    void MovePlayer()
    {
        // 青エリア内をタッチした場合のみ移動先座標を更新する
        Vector2 moveDestination = Update_moveDestinationIfBlueAreaTouched();

        if (Input.touchCount > 0)
        {
            // 移動方向(°)を設定する
            float moveAngle = 0.0f;
            moveAngle = GetAngleOf_moveDestination(this.transform.position, moveDestination);

            // 移動方向に対してPlayerを動かす
            base.SetVelocityForRigidbody2D(moveAngle, base.characterSpeed);
        }

        // Playerがタッチした座標に近づいたら止める
        this.StopPlayerTo_moveDestination(moveDestination);
    }

    /// <summary>
    /// 機能：青エリア内をタッチした場合のみ移動先座標を更新する
    ///
    /// 引数：なし
    ///
    /// 戻り値：なし
    ///
    /// 備考：参考メソッド：BaseCharactersMovementByTouch.StopCharacter()
    /// </summary>
    Vector2 Update_moveDestinationIfBlueAreaTouched()
    {
        Vector2 moveDestination = Vector2.zero;

        // 青エリア内をタッチした場合のみPlayerの移動先座標を更新する
        if (
            (BattleArea.myLeftSide <= base.touchPosition.x && base.touchPosition.x <= BattleArea.myRightSide)
            &&
            (BattleArea.myBottomSide <= base.touchPosition.y && base.touchPosition.y <= BattleArea.myTopSide)
            )
        {
            return moveDestination = this.touchPosition + new Vector2(BattleArea.distancePlayerAndTouch, 0.0f);
        }
        else
        {
            return Vector2.zero;
        }

    }

    /// <summary>
    /// 機能：Playerがタッチした座標に近づいたら止める
    ///
    /// 引数：Vector2 moveDestination：移動先の座標
    ///
    /// 戻り値：なし
    ///
    /// 備考：参考メソッド：BaseCharactersMovementByTouch.StopCharacter()
    /// </summary>
    void StopPlayerTo_moveDestination(Vector2 moveDestination)
    {
        // タッチ座標からの差
        float touchDif = 0.5f;

        // Playerがタッチ座標を原点とする矩形以内に入った場合
        if ((
            (moveDestination.x - touchDif <= this.transform.position.x) && (this.transform.position.x <= moveDestination.x + touchDif)
            )
            &&
            (
            (moveDestination.y - touchDif <= this.transform.position.y) && (this.transform.position.y <= moveDestination.y + touchDif)
            ))
        {
            // Playerの座標を移動先座標へ設定する
            this.transform.position = moveDestination;

            // Playerの動きを停止する
            this.rigidbody2D.velocity = Vector2.zero;
        }
    }

    // <summary>
    /// 機能：青エリア内をタッチした場合のみタッチした座標を更新する。
    /// 
    /// 引数：なし
    /// 
    /// 戻り値：なし
    /// </summary>
    void Update_touchPositionIfTouchedOnBlueArea()
    {
        // 1本指でタッチした場合
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // -------------------------デバッグ中-------------------------//
            // TextUIを見つける
            //GameObject textUI = GameObject.Find("touch.phase_Text");

            //// textコンポーネントにtouch.phaseを表示する
            //textUI.GetComponent<Text>().text = "タッチ状態は" + touch.phase;

            // -------------------------デバッグ中-------------------------//

            // タッチした座標を持つローカル変数
            Vector2 temp_touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            // 青エリア内をタッチした場合のみタッチした座標を更新する
            if (
                (BattleArea.myLeftSide <= temp_touchPosition.x && temp_touchPosition.x <= BattleArea.myRightSide)
                &&
                (BattleArea.myBottomSide <= temp_touchPosition.y && temp_touchPosition.y <= BattleArea.myTopSide)
                )
            {
                base.touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            }
        }
    }
    #endregion メソッド
}