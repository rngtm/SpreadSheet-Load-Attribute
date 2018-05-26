namespace GoogleSpreadSheet
{
    // https://developers.google.com/sheets/guides/concepts?hl=ja
    public class Settings
    {
        public const string ApiKey = ""; // Google API Consoleで作成したAPIキーをここで指定します。
        public const string SheetId = ""; // GoogleスプレッドシートのsheetIDをここで指定します。
        public const string Range = ""; // 取得範囲をここで指定します。 ("シート1"など)

        public static bool Active { get { return true; } } // これをfalseにすると変数の上書きが無効になります
    }
}