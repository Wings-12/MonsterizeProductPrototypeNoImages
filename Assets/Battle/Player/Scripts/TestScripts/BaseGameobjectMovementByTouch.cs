﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 画面上のゲームオブジェクトをタッチ操作で移動できるクラス
/// </summary>
public class BaseGameobjectMovementByTouch : MonoBehaviour
{
    #region フィールド
    ///<summary>
    ///ゲームオブジェクトのrigidbody
    ///</summary>
    new protected Rigidbody2D rigidbody2D;

    ///<summary>
    ///タッチ入力したときの移動先の座標
    ///</summary>
    protected Vector2 touchPosition;

    ///<summary>
    ///ゲームオブジェクトの移動スピード
    ///</summary>
    protected float characterSpeed;
    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        this.rigidbody2D = GetComponent<Rigidbody2D>();

        this.transform.position = Vector2.zero;

        this.touchPosition = Vector2.zero;

        this.characterSpeed = 28.0f;
    }

    /// <summary>
    /// ゲームオブジェクトを現在の座標からタッチした座標まで描画しながら移動する
    /// </summary>
    protected void MoveGameobject(Vector2 gameobjectPosition, Vector2 touchPosition)
    {
        Update_touchPosition();

        if (Input.touchCount > 0)
        {
            // 移動方向(°)
            float moveAngle = 0.0f;
            moveAngle = GetAngleOf_moveDestination(gameobjectPosition, touchPosition);

            // 移動方向に対してゲームオブジェクトを動かす
            SetVelocityForRigidbody2D(moveAngle, this.characterSpeed);
        }

        StopCharacter();
    }

    /// <summary>
    /// 機能：タッチした座標を更新する
    /// </summary>
    /// <remarks>備考：ゲームオブジェクトの移動先座標を更新するために使う</remarks>
    void Update_touchPosition()
    {
        if (Input.touchCount > 0)
        {
            // タッチオブジェクト
            Touch touch = Input.GetTouch(0);

            // タッチした座標を持つローカル変数
            this.touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        }
    }

    /// <summary>
    /// 機能：ゲームオブジェクトをタッチした座標に近づいたら止める
    void StopCharacter()
    {
        // タッチ座標からの差
        float touchDif = 0.5f;

        // ゲームオブジェクトがタッチ座標を原点とする矩形以内に入った場合
        if (
            (this.touchPosition.x - touchDif <= this.transform.position.x && this.transform.position.x <= this.touchPosition.x + touchDif)
            &&
            (this.touchPosition.y - touchDif <= this.transform.position.y && this.transform.position.y <= this.touchPosition.y + touchDif)
            )
        {
            // ゲームオブジェクトの座標をタッチ座標へ設定する
            this.transform.position = this.touchPosition;

            // ゲームオブジェクトの動きを停止する
            this.rigidbody2D.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 機能：現在のゲームオブジェクト座標から移動先座標への角度を求める
    /// </summary>
    /// <param name="currentgameobjectPosition">現在のゲームオブジェクト座標</param>
    /// <param name="moveDestination">移動先座標</param>
    /// <returns>現在のゲームオブジェクト座標から移動先座標への移動方向</returns>
    /// <remarks>
    /// 備考：参考サイト：https://qiita.com/2dgames_jp/items/60274efb7b90fa6f986a
    /// </remarks>
    float GetAngleOf_moveDestination(Vector2 currentgameobjectPosition, Vector2 moveDestination)
    {
        // 隣辺
        float adjacent = moveDestination.x - currentgameobjectPosition.x;

        // 対辺
        float opposite = moveDestination.y - currentgameobjectPosition.y;

        // 角度(ラジアン)
        float rad = Mathf.Atan2(opposite, adjacent);

        // 角度(°)
        return rad * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 機能：ゲームオブジェクトにアタッチされたRigidbody2Dの移動方向と速度を設定する
    /// </summary>
    /// <param name="angleOfMoveDestination">現在のゲームオブジェクト座標から移動先座標への移動方向</param>
    /// <param name="speed">ゲームオブジェクトの移動スピード</param>
    /// <remarks>備考：参考サイト：https://qiita.com/2dgames_jp/items/60274efb7b90fa6f986a</remarks>
    void SetVelocityForRigidbody2D(float angleOfMoveDestination, float speed)
    {
        // 移動先までの速度
        Vector2 velocityOfMoveDestination;

        // 移動先座標への角度におけるcosを設定
        velocityOfMoveDestination.x = Mathf.Cos(Mathf.Deg2Rad * angleOfMoveDestination) * speed;

        // 移動先座標への角度におけるsinを設定
        velocityOfMoveDestination.y = Mathf.Sin(Mathf.Deg2Rad * angleOfMoveDestination) * speed;

        // tan(斜辺の長さ(大きさ)とその向き == (cos(x座標), sin(y座標)))
        this.rigidbody2D.velocity = velocityOfMoveDestination;
    }
}