using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 概要：オブジェクトを破棄するクラス
/// 詳細：遠距離攻撃時に生成したオブジェクトを破棄する
/// </summary>
public class Shredder : MonoBehaviour
{
    /// <summary>
    /// 衝突したオブジェクトを破棄する
    /// </summary>
    /// <param name="collision">衝突したオブジェクト</param>
    /// <remarks>用途：遠距離攻撃時に生成したオブジェクトを破棄する</remarks>
    void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject);
    }
}
