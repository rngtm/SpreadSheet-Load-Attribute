namespace GoogleSpreadSheet
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Linq;
    using MessagePack;

    // https://developers.google.com/sheets/guides/concepts?hl=ja
    public static class SpreadSheetLoader
    {
        private static SpreadSheet _sheet; // 取得したGoogleスプレッドシート
        private static bool _isSuccess = false; // 取得に成功したかどうか
        private static bool _isLoading = false; // 取得に成功したかどうか
        private static Dictionary<string, List<string>> _dictionary;

        /// <summary>
        /// シート取得中
        /// </summary>
        public static bool IsLoading
        {
            get { return _isLoading; }
        }
        
        /// <summary>
        /// 取得に成功
        /// </summary>
        public static bool IsSuccess
        {
            get { return _isSuccess; }
        }

        /// <summary>
        /// 値の取得
        /// </summary>
        public static string GetValue(string key)
        {
            if (_dictionary.ContainsKey(key))
            {
                return _dictionary[key][1];
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// APIキーを使用したGoogleスプレッドシートの取得
        /// </summary>
        public static IEnumerator LoadCoroutine(string apiKey, string sheetId, string range)
        {
            _isLoading = true;
            _isSuccess = false;
            
            var api = "https://sheets.googleapis.com/v4/spreadsheets/" + sheetId + "/values/" + range + "?key=" +
                      apiKey;
            using (var request = UnityWebRequest.Get(api))
            {
                yield return request.SendWebRequest();

                if (request.isHttpError)
                {
                    Debug.LogErrorFormat("【HTTP Error】");
                    Debug.LogErrorFormat("{0}", request.downloadHandler.text);
                    _isLoading = false;
                    _isSuccess = false;
                    yield break;
                }

                if (request.isNetworkError)
                {
                    Debug.LogErrorFormat("【Network Error】");
                    Debug.LogErrorFormat("{0}", request.downloadHandler.text);
                    _isLoading = false;
                    _isSuccess = false;
                    yield break;
                }

                var json = request.downloadHandler.text;
                var bytes = MessagePackSerializer.FromJson(json);
                _sheet = MessagePackSerializer.Deserialize<SpreadSheet>(bytes);
                _dictionary = _sheet.Values.ToDictionary(item => item[0], item => item); // 左のセルをキーとするDictionaryを作成

                _isLoading = false;
                _isSuccess = true;
            }
        }
    }
}