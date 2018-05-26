using System.Collections.Generic;

namespace GoogleSpreadSheet
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SpreadSheetManager : MonoBehaviour
    {        
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            if (!Settings.Active)
            {
                return; 
            }
            
            new GameObject("SpreadSheet Manager").AddComponent<SpreadSheetManager>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // シート取得
            StartCoroutine(GetSheetCoroutine());
        }

        /// <summary>
        /// 取得を行う
        /// </summary>
        IEnumerator GetSheetCoroutine()
        {
            yield return SpreadSheetLoader.LoadCoroutine();
            
            if (!SpreadSheetLoader.IsSuccess)
            {
                // 通信に失敗した場合は終了
                yield break;
            } 
            
            SceneObjectUpdater.UpdateObjectValues(); // シーン内に存在するコンポーネントのメンバ値を更新
            
            SceneManager.activeSceneChanged += (from, to) => { OnChangeScene(); };
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