using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 概要：遠距離攻撃の衝突処理クラス
/// </summary>
public class LongRangeAttackCollision : MonoBehaviour
{
    /// <summary>
    /// 機能：Enemyと衝突した場合に遠距離攻撃を削除してファイアーボールのアニメーションを開始準備する
    /// </summary>
    /// <param name="collision">衝突したオブジェクト</param>
    /// <remarks>EnemyController上でファイアーボールのアニメーションを開始する</remarks>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Destroy(this.gameObject);

            // EnemyController上で被ダメージアニメーションを開始準備
            PlayerAttackSkill.isFireBallHit = true;
        }
    }

}
