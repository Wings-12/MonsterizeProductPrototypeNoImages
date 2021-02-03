using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// 概要：EnemyのHPが0になったときに再生するリザルト動画を操作するクラス
/// </summary>
public class ResultVideoController : MonoBehaviour
{
    /// <summary>
    /// enemyのHPを0にしたときに再生するリザルト動画
    /// </summary>
    VideoPlayer resultVideo;

    /// <summary>
    /// Enemyを操作するクラスのインスタンス
    /// </summary>
    /// <remarks>用途：OnBattleResultイベントにOnBattleResultEventHandlerを設定するために使用</remarks>
    [SerializeField] EnemyController enemyController = default;

    // Use this for initialization
    void Start()
    {
        this.resultVideo = GetComponent<VideoPlayer>();

        // OnBattleResultイベントにOnBattleResultEventHandlerを設定
        this.enemyController.OnBattleResult += OnBattleResultEventHandler;

        // Result Videoゲームオブジェクトを非アクティブにして非表示にする
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 機能：Enemyを倒した時にバトル終了を分かりやすくする演出をするための動画を流すイベントハンドラ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <remarks>備考：Enemyを倒した時に発生するイベントハンドラ</remarks>
    void OnBattleResultEventHandler(object sender, System.EventArgs e)
    {
        //　再生中でなければ再生する
        if (!this.resultVideo.isPlaying)
        {
            // Result Videoゲームオブジェクトをアクティブにして表示する
            this.gameObject.SetActive(true);

            // リザルト動画を再生する
            this.resultVideo.Play();
        }
        else
        {
            // リザルト動画を一時停止する
            this.resultVideo.Pause();
        }
    }
}