using System.Collections.Generic;

namespace GoogleSpreadSheet
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SpreadSheetManager : MonoBehaviour
    {
        /// <summary>
        /// シート取得中かどうか
        /// </summary>
        public static bool IsLoading { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            new GameObject("SpreadSheet Manager").AddComponent<SpreadSheetManager>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // シート取得
            StartCoroutine(GetSheetCoroutine());
        }

        /// <summary>
        /// 取得テスト
        /// </summary>
        IEnumerator GetSheetCoroutine()
        {
            IsLoading = true;
            yield return SpreadSheetLoader.LoadCoroutine(Settings.ApiKey, Settings.SheetId, Settings.Range);
            
            if (!SpreadSheetLoader.IsSuccess) { yield break; } // 通信に失敗した場合は終了
            
            SceneObjectUpdater.UpdateObjectValues(); // シーン内に存在するコンポーネントのメンバ値を更新
            
            SceneManager.activeSceneChanged += (from, to) => { OnChangeScene(); };
            IsLoading = false;
        }

        /// <summary>
        /// シーン変更時に呼ばれる
        /// </summary>
        private void OnChangeScene()
        {
            SceneObjectUpdater.UpdateObjectValues();
        }
    }
}