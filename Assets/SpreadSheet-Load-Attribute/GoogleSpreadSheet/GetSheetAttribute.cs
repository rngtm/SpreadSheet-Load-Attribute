namespace GoogleSpreadSheet
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class GetSheetAttribute : Attribute
    {
        private string _key = ""; // 取得対象の行のキー

        public string Key
        {
            get { return _key; }
        }

        public GetSheetAttribute()
        {
        }

        public GetSheetAttribute(string key)
        {
            _key = key;
        }
    }
}