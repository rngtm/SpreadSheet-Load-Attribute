namespace GoogleSpreadSheet
{
    using MessagePack;
    using System.Collections.Generic;

    [MessagePackObject]
    public class SpreadSheet
    {
        [Key("range")] public string Range { get; set; }

        [Key("majorDimension")] public string MajorDimension { get; set; }

        [Key("values")] public List<List<string>> Values { get; set; }
    }
}