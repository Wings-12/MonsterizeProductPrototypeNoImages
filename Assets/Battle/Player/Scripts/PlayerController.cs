using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 概要：Playerを操作するクラス
/// 詳細：①移動 ②遠距離攻撃 ③近距離攻撃(実装途中)
/// </summary>
public class PlayerController : BaseCharactersMovementByTouch
{
    #region フィールド

    #region 移動のためのメンバ変数
    ///<summary>
    ///Playerの初期座標
    ///</summary>
    readonly Vector2 iniPlayerPosition = new Vector2(-5.0f, -1.0f);

    #endregion 移動のためのメンバ変数

    #region 攻撃処理のためのメンバ変数

    /// <summary>
    /// Player攻撃技インスタンス
    /// </summary>
    [SerializeField] PlayerAttackSkill playerAttackSkill = default;

    /// <summary>
    /// タッチ開始時の座標
    /// </summary>
    Vector2 touchPhaseBeganPosition;

    /// <summary>
    /// TouchPhase.Endedが検知されたときの座標を保持する変数
    /// </summary>
    Vector2 touchPhaseEndedPosition;

    /// <summary>
    /// 近距離攻撃(相手エリアで右から左へフリック)されたか判定するbool変数
    /// </summary>
    /// <remarks>用途：true:近距離攻撃された / false:近距離攻撃されてない</remarks>
    bool isCloseRangeAttack;

    /// <summary>
    /// 近距離攻撃のために移動する前の座標
    /// </summary>
    /// <remarks>近距離攻撃を終了してワープして戻ってくるときの座標として使用</remarks>
    Vector2 closeRangeAttackMoveStartPosition;

    /// <summary>
    /// 近距離攻撃のために移動する前の座標プロパティ
    /// </summary>
    /// <remarks>近距離攻撃を終了してワープして戻ってくるときの座標として使用</remarks>
    public Vector2 CloseRangeAttackMoveStartPosition
    {
        get
        {
            return this.closeRangeAttackMoveStartPosition;
        }
        set
        {
            this.closeRangeAttackMoveStartPosition = value;
        }
    }

    /// <summary>
    /// 近距離攻撃をEnemyに当てるときの座標
    /// </summary>
    Vector2 closeRangeAttackPosition;

    /// <summary>
    /// 近距離攻撃をEnemyに当てるときの座標プロパティ
    /// </summary>
    public Vector2 CloseRangeAttackPosition
    {
        get
        {
            return this.closeRangeAttackPosition;
        }
        set
        {
            this.closeRangeAttackPosition = value;
        }
    }

    /// <summary>
    /// 遠距離攻撃(相手エリアでタップ)されたかどうか判定するbool変数
    /// </summary>
    /// <remarks>
    /// true：遠距離攻撃された / false；されなかった
    /// </remarks>
    bool isLongRangeAttack;

    /// <summary>
    /// タップが開始されたかどうか判定するbool変数
    /// </summary>
    /// <remarks>タップが開始された / false：タップが開始されていない</remarks>
    bool isTapingBegan;

    /// <summary>
    /// ひっかく攻撃で移動中は近距離攻撃処理をしないbool変数
    /// </summary>
    bool isWaitForSecondsForScrach;

    /// <summary>
    /// ひっかく攻撃で移動中は近距離攻撃処理をしないbool変数のプロパティ
    /// </summary>
    public bool IsWaitForSecondsForScrach
    {
        get
        {
            return this.isWaitForSecondsForScrach;
        }
        set
        {
            this.isWaitForSecondsForScrach = value;
        }
    }

    #endregion 攻撃処理のためのメンバ変数

    #endregion フィールド

    #region メソッド

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        // Playerの座標を初期化
        this.transform.position = this.iniPlayerPosition;

        // タッチ座標は画面左側ではない、テキトーな座標で設定して、ゲーム開始時にスタート地点からPlayerが動かないようにする
        base.touchPosition = new Vector2(9999.0f, 9999.0f);

        #region 攻撃処理のためのメンバ変数の初期化

        this.CloseRangeAttackMoveStartPosition = Vector2.zero;

        this.CloseRangeAttackPosition = Vector2.zero;

        this.touchPhaseBeganPosition = Vector2.zero;

        this.touchPhaseEndedPosition = Vector2.zero;

        this.isCloseRangeAttack = false;

        this.isLongRangeAttack = false;

        this.isTapingBegan = false;

        this.IsWaitForSecondsForScrach = false;

        #endregion 攻撃処理のためのメンバ変数の初期化
    }

    // Update is called once per frame
    new void Update()
    {
        // 近距離攻撃処理がされ、かつ近距離攻撃処理の待機処理中にタッチ移動処理をしないようにする
        // 処理の意味：近距離攻撃処理時に移動処理がされてキャラクターが自エリア内から出られないことがないようにするため
        if (this.isCloseRangeAttack == false && this.IsWaitForSecondsForScrach == false)
        {
            // 自エリア内をタッチやスワイプで移動する
            MovePlayerInMyArea();
        }

        // LongRangeAttackメソッド通過後に近距離攻撃フラグを降ろして遠距離攻撃できるようにする
        // 備考：近距離攻撃するためにフリックしたときに遠距離攻撃も処理されてしまうバグ対策に追加
        this.isCloseRangeAttack = false;

        // 攻撃処理
        Attacks();
    }

    /// <summary>
    /// 機能：攻撃処理
    /// </summary>
    private void Attacks()
    {
        // タップ(遠距離攻撃)されていない、かつひっかく攻撃の待機処理をしていなければ近距離攻撃処理に入る
        if (this.isLongRangeAttack == false && this.IsWaitForSecondsForScrach == false)
        {
            CloseRangeAttack(); 
        }

        // CloseRangeAttackkメソッド通過後に遠距離攻撃フラグを降ろして近距離攻撃できるようにする
        // 備考：近距離攻撃するためにフリックしたときに遠距離攻撃も処理されてしまうバグ対策に追加
        this.isLongRangeAttack = false;

        // 近距離攻撃(敵エリアで右から左へフリック)されていなければ遠距離攻撃処理に入る
        if (this.isCloseRangeAttack == false)
        {
            LongRangeAttack();
        }

        
    }

    #region 移動メソッド

    /// <summary>
    /// 機能：自エリア内をタッチ操作で描画しながら移動する
    /// </summary>
    private void MovePlayerInMyArea()
    {
        // 自エリア内をタッチした場合のみタッチした座標を設定する
        Set_touchPositionIfTouchedOnMyArea();

        // メイン自エリア内をタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        // Playerをタッチよりも右側に移動する理由：Playerの指でPlayer(キャラクター)が隠れないようにするため
        if (
        (BattleArea.myLeftSide < base.touchPosition.x && base.touchPosition.x < BattleArea.mySecondRightSide)
        &&
        (BattleArea.myTopSide > base.touchPosition.y && base.touchPosition.y > BattleArea.myBottomSide)
        )
        {
            MovePlayer();
        }

        // 右自エリアをタッチした場合にPlayerをタッチ座標よりも右側に描画しながら移動する
        // 注意：↑は厳密にはちょっと違う。詳細はMovePlayerInRightBlueAreaメソッドを参照
        else if (
            (BattleArea.mySecondRightSide <= base.touchPosition.x && base.touchPosition.x <= BattleArea.myRightSide)
            &&
                (BattleArea.myBottomSide < base.touchPosition.y && base.touchPosition.y < BattleArea.myTopSide)
                )
        {
            MovePlayerInRightBlueArea();
        }
    }

    /// <summary>
    /// 機能：右自エリア内をタッチした場合にPlayerをタッチ操作で描画しながら移動する
    /// </summary>
    /// <remarks>
    /// 備考：　
    /// 右自エリア内をタッチした場合の移動は大きく分けて以下の2通りある。
    /// 1. Playerが右自エリア内にいた場合にPlayerのx座標は自エリア右端に固定されて上下に移動する
    /// 2. Playerが右自エリア内より左側にいた場合にMovePlayerメソッドでタッチ座標より1マス右側に向けて移動する
    /// </remarks>
    void MovePlayerInRightBlueArea()
    {
        // Playerのx座標を固定して、y座標を自エリア右端に設定するローカル変数の初期化
        Vector2 playerPositionOnlyXCoordinateFixed = new Vector2(BattleArea.myRightSide, this.transform.position.y);

        // PlayerがtouchPositionに来た時に上下にぶれて動かないようにするための基準値
        float standardValueToStopPlayer = 0.5f;

        // Playerが右自エリアより左側にいた場合
        if (this.transform.position.x < BattleArea.mySecondRightSide)
        {
            MovePlayer();
        }

        // Playerより上側をタッチした場合
        else if (playerPositionOnlyXCoordinateFixed.y < base.touchPosition.y)
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
    /// 機能：自エリア内でPlayerをタッチ操作で移動先座標まで描画しながら移動する
    /// </summary>
    /// <remarks>
    /// 備考：このメソッドでPlayerの移動座標をタッチ操作で隠れない座標に更新しているので注意
    /// </remarks>
    void MovePlayer()
    {
        // 移動先座標
        Vector2 moveDestination = Vector2.zero;

        // 自エリア内をタッチした場合のみ移動先座標を更新する
        moveDestination = Get_moveDestinationIfMyAreaTouched();

        // 移動先座標が更新されていた場合
        if (moveDestination != Vector2.zero)
        {
            // 移動方向を取得する
            float moveAngle = 0.0f;
            moveAngle = GetAngleOf_moveDestination(this.transform.position, moveDestination);

            // 移動方向に対してPlayerを動かす
            base.SetVelocityForRigidbody2D(moveAngle, characterSpeed);
        
            // Playerがタッチした座標に近づいたら止める
            StopPlayerTo_moveDestination(moveDestination);
        }
    }

    /// <summary>
    /// 機能：自エリア内をタッチした場合のみ移動先座標を取得する
    /// </summary>
    Vector2 Get_moveDestinationIfMyAreaTouched()
    {
        // 移動先座標
        Vector2 moveDestination = Vector2.zero;

        // 自エリア内をタッチした場合のみPlayerの移動先座標を更新する
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
    /// </summary>
    /// <param name="moveDestination">移動先座標</param>
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

    /// <summary>
    /// 機能：自エリア内をタッチした場合のみタッチした座標を設定する
    /// </summary>
    void Set_touchPositionIfTouchedOnMyArea()
    {
        // 1本指でタッチした場合
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // タッチした座標を持つローカル変数
            Vector2 temp_touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            // 自エリア内をタッチした場合のみタッチした座標を更新する
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
    #endregion 移動メソッド

    #region 攻撃メソッド
    /// <summary>
    /// 機能：相手エリアをタッチしたら遠距離攻撃する
    /// </summary>
    /// <remarks>備考：2020/11/16時点での遠距離攻撃はファイアーボールを実装</remarks>
    void LongRangeAttack()
    {
        // タッチ情報の取得
        Touch[] myTouches = Input.touches;

        // タッチ数ぶんループ
        for (int i = 0; i < myTouches.Length; i++)
        {
            // タッチ座標のワールド座標を取得
            Vector2 attackTouchPosition = Camera.main.ScreenToWorldPoint(myTouches[i].position);

            // 相手エリアをタッチした場合にタップ開始フラグを上げる
            if (
                (BattleArea.enemyLeftSide < attackTouchPosition.x)
                &&
                (attackTouchPosition.y < BattleArea.enemyTopSide)
                &&
                (myTouches[i].phase == TouchPhase.Began)
                )
            {
                this.isTapingBegan = true;
            }

            // 相手エリアをタッチ終了した場合
            if (
                (BattleArea.enemyLeftSide < attackTouchPosition.x)
                &&
                (attackTouchPosition.y < BattleArea.enemyTopSide)
                &&
                (this.isTapingBegan == true)　&&　(myTouches[i].phase == TouchPhase.Ended)
                )
            {
                // タップ開始フラグを降ろす
                this.isTapingBegan = false;

                // このタッチはタップ(遠距離攻撃)と判定
                this.isLongRangeAttack = true;

                // ファイアーボール開始
                this.playerAttackSkill.StartFireBall();

                // ファイアーボールを1回呼んだらbreak
                break;
            }
        }
    }

    /// <summary>
    /// 機能：相手エリアを左から右へフリックしたら近距離攻撃する
    /// </summary>
    /// <remarks>
    /// </remarks>
    void CloseRangeAttack()
    {
        // 近距離攻撃終了後にPlayerが戻ってくる座標を設定
        this.CloseRangeAttackMoveStartPosition = this.transform.position;

        // タッチ情報の取得
        Touch[] myTouches = Input.touches;

        // // タッチ数ぶんループ
        for (int i = 0; i < myTouches.Length; i++)
        {
            // タッチ座標のワールド座標を取得
            this.CloseRangeAttackPosition = Camera.main.ScreenToWorldPoint(myTouches[i].position);

            /*以下相手エリアを右方向にフリックする処理*/

            // 右から左へフリック入力したと判定するための2点間の距離
            float distanceForFlickInput = 0.0f;

            // 相手エリアでタッチした場合
            if (
                (BattleArea.enemyLeftSide < this.CloseRangeAttackPosition.x)
                &&
                (this.CloseRangeAttackPosition.y < BattleArea.enemyTopSide)
                &&
                (myTouches[i].phase == TouchPhase.Began)
                )
            {
                // TouchPhase.Beganが検知されたときの座標を取得する
                this.touchPhaseBeganPosition = Camera.main.ScreenToWorldPoint(myTouches[i].position);
            }

            // フリック入力したかどうかの基準距離
            const float distanceIfFlicked = 0.3f;

            // 相手エリアでフリックされた場合
            if (
                (BattleArea.enemyLeftSide < this.CloseRangeAttackPosition.x)
                &&
                (this.CloseRangeAttackPosition.y < BattleArea.enemyTopSide)

                &&
                (myTouches[i].phase == TouchPhase.Ended)
                )
            {
                // TouchPhase.Endedが検知されたときの座標を取得する
                this.touchPhaseEndedPosition = Camera.main.ScreenToWorldPoint(myTouches[i].position);

                // TouchPhase.Beganが検知されたときのx座標と
                // TouchPhase.Endedが検知されたときのx座標の差分でフリック入力判定をするための距離を算出
                distanceForFlickInput = this.touchPhaseEndedPosition.x - this.touchPhaseBeganPosition.x;

                // フリック入力の基準距離を超えていなければ、フリック入力(近距離攻撃)はされなかったと判定する
                if (distanceForFlickInput <= distanceIfFlicked)
                {
                    this.isCloseRangeAttack = false;

                    break;
                }
            }

            // フリック入力(近距離攻撃)がされた場合
            if (distanceForFlickInput > distanceIfFlicked)
            {
                // 近距離攻撃開始
                this.isCloseRangeAttack = true;

                // 近距離攻撃(ひっかく)開始
                this.playerAttackSkill.Scratch();
            }
        }
    }
    #endregion 攻撃メソッド

    #endregion メソッド
}
