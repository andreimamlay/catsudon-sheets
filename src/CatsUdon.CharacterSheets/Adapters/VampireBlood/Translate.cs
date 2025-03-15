namespace CatsUdon.CharacterSheets.Adapters.VampireBlood;

public static class Translate
{
    public static string ToJp(NechronicaConverter.PartTypes partType) => partType switch
    {
        NechronicaConverter.PartTypes.None => string.Empty,
        NechronicaConverter.PartTypes.Position => "ポジション",
        NechronicaConverter.PartTypes.MainClass => "メインクラス",
        NechronicaConverter.PartTypes.SubClass => "サブクラス",
        NechronicaConverter.PartTypes.Head => "頭部",
        NechronicaConverter.PartTypes.Arm => "腕",
        NechronicaConverter.PartTypes.Body => "胴",
        NechronicaConverter.PartTypes.Leg => "足",
        _ => throw new ArgumentOutOfRangeException(nameof(partType), partType, null)
    };

    public static string ToJp(NechronicaConverter.Timings timing) => timing switch
    {
        NechronicaConverter.Timings.Auto => "オート",
        NechronicaConverter.Timings.Rapid => "ラピッド",
        NechronicaConverter.Timings.Judge => "ジャッジ",
        NechronicaConverter.Timings.Damage => "ダメージ",
        NechronicaConverter.Timings.Action => "アクション",
        _ => throw new ArgumentOutOfRangeException(nameof(timing), timing, null)
    };
}
