using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 概要：Playerの攻撃技クラス
/// </summary>
public class PlayerAttackSkill : BaseGameobjectMovementByTouch
{
    #region フィールド

    #region ファイアーボール処理で使うメンバ変数
    /// <summary>
    /// ファイアーボールが生成される座標
    /// </summary>
    Vector2 fireBallStartPosition;

    /// <summary>
    /// Playerのゲームオブジェクト
    /// </summary>
    GameObject player;

    /// <summary>
    /// PlayerControllerのインスタンス
    /// </summary>
    PlayerController playerController;

    /// <summary>
    /// Animatorのインスタンス
    /// </summary>
    [SerializeField] Animator animator = default;

    /// <summary>
    /// ファイアーボールのプレハブ
    /// </summary>
    [SerializeField] GameObject fireBall_prefab = default;

    /// <summary>
    /// ファイアーボールのゲームオブジェクト
    /// </summary>
    GameObject fireBall = null;

    /// <summary>
    /// ファイアーボール発射音
    /// </summary>
    [SerializeField] AudioClip soundForFiringFireBall = default;

    /// <summary>
    /// AudioSourceのインスタンス
    /// </summary>
    AudioSource audioSource;

    /// <summary>
    /// ファイアーボール時にWaitForSecondsで待機処理がされているかどうか判定するbool変数
    /// </summary>
    /// <remarks>
    /// 用途：true：WaitForSecondsで待機処理がされている / false：されていない
    /// 意図：Enemyに遠距離攻撃が当たっても連射できないようにCoroutineFireBallメソッドを停止するために追加</remarks>
    bool isWaitForSecondsForFireBall;

    /// <summary>
    /// Enemyにぶつかったかどうか判定するbool変数
    /// </summary>
    /// <remarks>true：ぶつかった / false：ぶつかってない</remarks>
    bool isCollidingEnemy;

    /// <summary>
    /// Enemyにひっかく攻撃を当てたかどうか判定するbool変数
    /// </summary>
    /// <remarks>true：Enemyにひっかく攻撃を当てた / false：当ててない</remarks>
    public static bool isScrachingEnemy = false;

    /// <summary>
    /// Playerがファイアーボールを当てたかどうかを判定するbool変数
    /// </summary>
    /// <remarks>用途：true：ファイアーボールを当てた / false：当てていない</remarks>
    public static bool isFireBallHit = false;


    #endregion ファイアーボール処理で使うメンバ変数

    #endregion フィールド

    #region メソッド
    // Start is called before the first frame update
    new void Start()
    {
        this.player = GameObject.Find("Player");

        this.playerController = this.player.GetComponent<PlayerController>();

        this.audioSource = this.gameObject.GetComponent<AudioSource>();

        this.isWaitForSecondsForFireBall = false;

        this.isCollidingEnemy = false;
    }

    /// <summary>
    /// ファイアーボールのメイン処理
    /// </summary>
    IEnumerator CoroutineFireBall()
    {
        // ファイアーボールのアニメーション開始
        this.animator.Play("BabyFlame");

        // ファイアーボール発射座標をアップデート
        // 理由：ファイアーボールがPlayerから発射されるように見せるため
        this.fireBallStartPosition =
            (Vector2)this.playerController.transform.position +
            new Vector2(2.5f, 0.0f);

        // ファイアーボールのprefab生成
        this.fireBall = Instantiate(
            this.fireBall_prefab,
            this.fireBallStartPosition,
            Quaternion.identity);

        // ファイアーボールへ瞬間的に力を加えて右方向に飛ばす
        Vector2 force = new Vector2(30.0f, 0.0f);
        this.fireBall.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);

        // ファイアボールの発射音を鳴らす
        this.audioSource.PlayOneShot(soundForFiringFireBall);

        // CoroutineFireBallの処理をこれ以上呼べないようにする
        // 理由：すぐ下のWaitForSeconds処理がされている間はまたCoroutineFireBallが呼ばれてファイアーボールを連射できないようにするため
        this.isWaitForSecondsForFireBall = true;

        // 遠距離攻撃を終了して近距離攻撃をできるようにする
        //this.playerController.IsLongRangeAttack = false;

        // CoroutineFireBall()の処理を指定秒数止める
        // 理由：ファイアーボールを高速で連射できないようにするため
        yield return new WaitForSeconds(0.3f);

        // WaitForSeconds処理が終わったら次のファイアーボールが撃てるようにする
        this.isWaitForSecondsForFireBall = false;
    }

    /// <summary>
    /// ファイアーボールの処理を開始する
    /// </summary>
    public void StartFireBall()
    {
        // Enemyにファイアーボールが当たっていないかつファイアーボールが生成されていない場合
        if (!this.isWaitForSecondsForFireBall && !fireBall)
        {
            // ファイアーボールの処理開始
            StartCoroutine(CoroutineFireBall());
        }
    }

    /// <summary>
    /// ひっかく攻撃のメイン処理
    /// </summary>
    IEnumerator CoroutineScratch()
    {
        // Playerダッシュアニメーションをする
        this.animator.Play("Dash");

        // Playerをダッシュする
        Vector2 force = new Vector2(60.0f, 0.0f);
        this.player.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);

        // 待機処理中は移動も近距離攻撃もできないようにする
        this.playerController.IsWaitForSecondsForScrach = true;

        // Playerが近距離攻撃位置まで移動するまで待つ
        while (this.player.transform.position.x < this.playerController.CloseRangeAttackPosition.x)
        {
            // Enemyにぶつかったら待機処理終了
            if (this.isCollidingEnemy == true)
            {
                this.isCollidingEnemy = false;

                break;
            }

            yield return null;
        }

        // ダッシュを止める
        this.player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // ひっかく攻撃のアニメーションを開始する
        this.animator.Play("Scratch");

        // Scratchのステートの反映に1フレーム使う
        yield return null;

        // ひっかく攻撃のアニメーションが終わるまで待機する
        // ※0.7fくらいでひっかく攻撃のアニメーションが終了している模様
        while (this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
        {
            yield return null;
        }

        // ひっかく攻撃を当てたのでEnemyControllerクラスのTakeDamageByScratchでダメージ処理できるようにする
        isScrachingEnemy = true;

        // 近距離攻撃する前の自エリアの座標までワープして戻る
        this.player.transform.position = this.playerController.CloseRangeAttackMoveStartPosition;

        // WaitForSeconds処理が終わったら次のひっかく攻撃ができるようにする
        this.playerController.IsWaitForSecondsForScrach = false;
    }

    /// <summary>
    /// ひっかく攻撃
    /// </summary>
    /// <remarks>
    /// ステータス：近距離攻撃実装中
    /// 仕様：敵エリア内で右方向にフリック入力したら、
    /// 最初に敵エリアをタッチした座標までPlayerが直線的に移動して
    /// ひっかく攻撃をする。
    /// その後、ひっかく攻撃のために移動する前の自エリアの座標までワープして戻る。
    /// </remarks>
    public void Scratch()
    {
        StartCoroutine(CoroutineScratch());
    }

    /// <summary>
    /// Enemyと衝突したかどうかを判定する
    /// </summary>
    /// <param name="collision"></param>
    /// <remarks>備考：Playerがひっかく攻撃で相手エリアに移動している際にEnemyと衝突したときに、Playerの移動を止めるために使用</remarks>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Enemy")
        {
            this.isCollidingEnemy = true;
        }
    }
    #endregion メソッド
}
