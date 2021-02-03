using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 概要：オブジェクトを破棄するクラス
/// 詳細：遠距離攻撃時に生成したオブジェクトを破棄する
/// </summary>
public class BGM : MonoBehaviour
{
    /// <summary>
    /// AudioClipのインスタンス
    /// </summary>
    [SerializeField] AudioClip audioClip = default;
    
    /// <summary>
    /// AudioSourceのインスタンス
    /// </summary>
    AudioSource audioSource;

    /// <summary>
    /// Enemyを操作するクラスのインスタンス
    /// </summary>
    [SerializeField] EnemyController enemyController = default;

    // Start is called before the first frame update
    void Start()
    {
        // BGMを流す
        this.audioSource = gameObject.GetComponent<AudioSource>();
        this.audioSource.PlayOneShot(this.audioClip);

        // OnStoppingBGMイベントにOnStoppingBGMEventHandlerを設定
        this.enemyController.OnStoppingBGM += OnStoppingBGMEventHandler;
    }

    /// <summary>
    /// BGMを停止するイベントハンドラ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnStoppingBGMEventHandler(object sender, System.EventArgs e)
    {
        this.audioSource.Stop();
    }
}
