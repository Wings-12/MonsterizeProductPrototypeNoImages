using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 概要：Enemyを操作するクラス
/// 詳細：①ランダム移動　②HP処理　③HPが0になった場合の戦闘結果処理
/// </summary>
/// <remarks>
/// 実装予定機能：
/// ①Playerのファイアボールが向かってきたらよける
/// ②遠距離攻撃
/// ③近距離攻撃
/// </remarks>
public class EnemyController : MonoBehaviour
{
    #region フィールド
    /// <summary>
    /// Animatorのインスタンス
    /// </summary>
    [SerializeField] Animator animator = default;

    /// <summary>
    /// 戦闘結果イベント
    /// </summary>
    public event EventHandler OnBattleResult;

    /// <summary>
    /// BGM停止イベント
    /// </summary>
    public event EventHandler OnStoppingBGM;

    /// <summary>
    /// ファイアーボールがEnemyに着弾したときのSE
    /// </summary>
    [SerializeField] AudioClip fireballExplosion = default;

    /// <summary>
    /// AudioSourceのインスタンス
    /// </summary>
    AudioSource audioSource;

    /// <summary>
    /// Enemyの体力バー
    /// </summary>
    /// <remarks>備考：0～1で推移</remarks>
    Slider hpBar;

    /// <summary>
    /// WaitForSecondsで待機処理がされているかどうか判定するbool変数
    /// </summary>
    /// <remarks>用途：true:WaitForSecondsで待機処理がされている / false:されてない
    /// 意図：何度もCoroutineMoveEnemyAtRandomメソッドを呼ばないようにするため
    /// </remarks>
    bool isWaitForSeconds;

    #endregion フィールド

    #region メソッド
    // Start is called before the first frame update
    void Start()
    {
        this.audioSource = this.gameObject.GetComponent<AudioSource>();

        this.hpBar = GameObject.Find("HP Bar").GetComponent<Slider>();

        this.hpBar.value = 1.0f;

        this.isWaitForSeconds = false;
    }

    // Update is called once per frame
    void Update()
    {
        // ファイアーボールでダメージを受けた場合の処理
        TakeDamageByFireBall();

        // ひっかく攻撃でダメージを受けた場合の処理
        TakeDamageByScratch();

        /* 実装予定-------------------------------------------------------------- */
        // 後でHPが0になったらわかりやすいやられた～演出を入れる
        //if (remainingHealth == 0)
        /* 実装予定-------------------------------------------------------------- */

        // HPが0になった場合の処理
        DropHPTo0();

        // 敵を移動する
        MoveEnemy();
    }

    /// <summary>
    /// 機能：敵を移動する
    /// </summary>
    /// <remarks>備考：現状とりあえずランダムで敵エリア内を一定テンポでワープして移動する</remarks>
    void MoveEnemy()
    {
        // 待機処理(WaitForSeconds)がされていない場合に
        if (this.isWaitForSeconds == false)
        {
            StartCoroutine(CoroutineMoveEnemyAtRandom());
        }
    }

    /// <summary>
    /// HPが0になった場合の処理
    /// </summary>
    void DropHPTo0()
    {
        // HPが0になった場合
        if (this.hpBar.value == 0.0f)
        {
            // Enemyを破棄する
            Destroy(this.gameObject);

            // BGMを止める
            if (this.OnStoppingBGM != null)
            {
                this.OnStoppingBGM(this, EventArgs.Empty);
            }

            // 戦闘結果処理
            if (this.OnBattleResult != null)
            {
                this.OnBattleResult(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// 機能：ひっかく攻撃でダメージを受けた場合の処理
    /// </summary>
    void TakeDamageByScratch()
    {
        // Playerがひっかく攻撃を当てた場合
        if (PlayerAttackSkill.isScrachingEnemy == true)
        {
            // ひっかく攻撃を当ててない場合に処理されないようにする
            PlayerAttackSkill.isScrachingEnemy = false;

            // 被弾アニメーション(爆発)を再生
            // ※まだアニメーションを作ってないので代用
            this.animator.Play("Explosion");

            // 爆発音を再生
            // ※まだSEを作ってないので代用
            this.audioSource.PlayOneShot(this.fireballExplosion);

            // HPバーの大きさを50%ダメージ分小さくする
            this.hpBar.value -= 0.5f;
        }
    }

    /// <summary>
    /// 機能：ファイアーボールでダメージを受けた場合の処理
    /// </summary>
    void TakeDamageByFireBall()
    {
        // Playerがファイアーボールを当てた場合
        if (PlayerAttackSkill.isFireBallHit == true)
        {
            // ファイアーボールを当ててない場合に処理されないようにする
            PlayerAttackSkill.isFireBallHit = false;

            // 被弾アニメーション(爆発)を再生
            this.animator.Play("Explosion");

            // 爆発音を再生
            this.audioSource.PlayOneShot(this.fireballExplosion);

            // HPバーの大きさを10%ダメージ分小さくする
            this.hpBar.value -= 0.1f;
        }
    }

    /// <summary>
    /// ランダムで敵エリア内を一定テンポでワープして移動する
    /// </summary>
    IEnumerator CoroutineMoveEnemyAtRandom()
    {
        // 待機処理(WaitForSeconds)が走っている場合はtrueにして
        // 何度もUpdateメソッドの中でStartCoroutine(CoroutineMoveEnemyAtRandom());を呼ばないようにする
        this.isWaitForSeconds = true;

        // Enemyがランダムに移動する範囲を敵エリア内に設定
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        float xCoordinate = UnityEngine.Random.Range(BattleArea.enemyLeftSide, BattleArea.enemyRightSide);
        float yCoordinate = UnityEngine.Random.Range(BattleArea.enemyBottomSide, BattleArea.enemyTopSide);

        // 相手エリア内をランダムにワープして移動する
        Vector2 enemyRandomPosition = new Vector2(xCoordinate, yCoordinate);
        this.transform.position = enemyRandomPosition;

        // ここでEnemyの動きが指定秒数止まる
        yield return new WaitForSeconds(0.4f);

        // 指定秒待機し終えたのでfalseにしてEnemyがランダム移動できるようにする。
        this.isWaitForSeconds = false;
    }
    #endregion メソッド
}