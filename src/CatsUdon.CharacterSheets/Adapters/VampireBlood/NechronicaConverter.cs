using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using CatsUdon.CharacterSheets.Memo;
using CatsUdon.CharacterSheets.TextSheets;
using System.Diagnostics;
using System.Text;

namespace CatsUdon.CharacterSheets.Adapters.VampireBlood;

public static class NechronicaConverter
{
    public static CharacterSheet Convert(IHtmlDocument document)
    {
        var characterData = new CCFoliaCharacterClipboardData();

        var character = new Character();
        ReadCharacter(character, document);

        return new CharacterSheet()
        {
            Character = ConvertToCCFoliaCharacter(character),
            AdditionalTextSheets = CreateTextSheets(character)
        };
    }

    private static CCFoliaCharacterClipboardData ConvertToCCFoliaCharacter(Character character)
    {
        var ccFoliaCharacter = new CCFoliaCharacter()
        {
            Name = character.Name,
            Initiative = character.ActionValue
        };

        ccFoliaCharacter.Status.Add(new CCFoliaStatus() { Label = "狂気度回復数", Value = 0, Max = 0 });
        foreach (var attachment in character.LingeringAttachments)
        {
            ccFoliaCharacter.Status.Add(new CCFoliaStatus() { Label = $"{LimitLength(attachment.Target, 8)}", Value = attachment.InsanityLevel, Max = 4 });
        }

        ccFoliaCharacter.Memo = CreateMemo(character);
        ccFoliaCharacter.Commands = CreateCommands(character);

        return new CCFoliaCharacterClipboardData()
        {
            Data = ccFoliaCharacter
        };

        string LimitLength(string value, int length)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value[..Math.Min(value.Length, length)];
        }
    }

    private static string CreateCommands(Character character)
    {
        var commands = new StringBuilder();

        commands.AppendLine("!!! DiceBotを【ネクロニカ】にしてください !!!");
        commands.AppendLine("nm 姉妹への未練表");
        commands.AppendLine("nmn 中立者への未練表");
        commands.AppendLine("nme 敵への未練表");

        return commands.Replace("\r\n", "\n").ToString().Trim();

    }

    private static string CreateMemo(Character character)
    {
        var memoBuilder = new MemoBuilder();

        memoBuilder.BeginSize(12);

        memoBuilder.Header("マニューバ");

        var timingOrder = new[] { Timings.Rapid, Timings.Judge, Timings.Damage, Timings.Action, Timings.Auto };
        foreach (var (index, timing) in timingOrder.Index())
        {
            
            if (index > 0)
            {
                memoBuilder.NewLine().NewLine();
            }

            memoBuilder.SubHeader(Translate.ToJp(timing));

            var parts = character.Parts.Where(p => p.Timing == timing).DistinctBy(p => p.Name).ToArray();
            if (parts.Length == 0)
            {
                memoBuilder.Text("なし");
                continue;
            }

            foreach (var part in parts)
            {
                memoBuilder
                    .Text($"▷ {Translate.ToJp(part.Type)} 【{part.Name}】")
                    .TextPrefixedOptional("  コスト", part.Cost)
                    .TextPrefixedOptional("  射程", part.Range);

                if (!string.IsNullOrWhiteSpace(part.Effect))
                {
                    memoBuilder.NewLine().Text("     ").Text(part.Effect);
                }

                memoBuilder.NewLine();
            }
        }

        memoBuilder.EndSize();

        return memoBuilder.ToString();
    }

    private static List<string> CreateTextSheets(Character character)
    {
        var sheets = new List<string>();

        var maxPartsInGroup = character.Parts.GroupBy(p => p.Type).Max(g => g.Count());
        var grid = new Grid();
        
        var types = new[] { PartTypes.Head, PartTypes.Arm, PartTypes.Body, PartTypes.Leg };
        foreach (var (col, type) in types.Index())
        {
            ref var cell = ref grid.Cells[0, col];
            cell.Text = Translate.ToJp(type);
            cell.Pushed = true;
            cell.Checked = true;
        }

        foreach (var (col, type) in types.Index())
        {
            var parts = character.Parts.Where(p => p.Type == type);
            foreach (var (row, part) in parts.Index())
            {
                ref var cell = ref grid.Cells[row + 1, col];
                cell.Text = part.Name;
                cell.Pushed = part.IsUsed;
                cell.Checked = part.IsBroken;
            }
        }

        sheets.Add(grid.ToString());

        return sheets;
    }

    private static void ReadCharacter(Character character,IHtmlDocument document)
    {
        character.Name = document.QuerySelectorValue(CssSelectors.CharacterName);
        character.ActionValue = int.Parse(document.QuerySelectorValue(CssSelectors.ActionValue));

        var partRows = document.QuerySelectorAll(CssSelectors.PartRows).Skip(1);
        foreach (var partRow in partRows)
        {
            var part = new Part();

            ReadPart(part, partRow);

            var isEmpty = string.IsNullOrWhiteSpace(part.Name) 
                && part.Category == PartCategories.None 
                && part.Type == PartTypes.None 
                && part.Timing == Timings.Auto 
                && string.IsNullOrWhiteSpace(part.Cost) 
                && string.IsNullOrWhiteSpace(part.Range)
                && string.IsNullOrWhiteSpace(part.Effect) 
                && string.IsNullOrWhiteSpace(part.Source);

            if (isEmpty) continue;

            character.Parts.Add(part);
        }

        var lingeringAttachmentRows = document.QuerySelectorAll(CssSelectors.LingeringAttachmentRows).Skip(1);
        foreach (var laRow in lingeringAttachmentRows)
        {
            var attachment = new LingeringAttachment
            {
                Target = laRow.QuerySelectorValue(CssSelectors.LingeringAttachment.Target),
                Type = laRow.QuerySelectorValue(CssSelectors.LingeringAttachment.Type),
                InsanityLevel = int.Parse(laRow.QuerySelectorValue(CssSelectors.LingeringAttachment.InsanityLevel)),
                InsanityName = laRow.QuerySelectorValue(CssSelectors.LingeringAttachment.InsanityName),
                InsanityEffect = laRow.QuerySelectorValue(CssSelectors.LingeringAttachment.InsanityEffect),
                Memo = laRow.QuerySelectorValue(CssSelectors.LingeringAttachment.Memo)
            };

            if (string.IsNullOrEmpty(attachment.Target)) continue;

            character.LingeringAttachments.Add(attachment);
        }


        static void ReadPart(Part part, IElement partRow)
        {
            part.Name = partRow.QuerySelectorValue(CssSelectors.Parts.Name);
            var categoryValue = partRow.QuerySelectorValue(CssSelectors.Parts.Category);
            if (int.TryParse(categoryValue, out var category)) 
            {
                part.Category = (PartCategories)category;
            }

            var typeValue = partRow.QuerySelectorValue(CssSelectors.Parts.Type);
            if (int.TryParse(typeValue, out var type))
            {
                part.Type = (PartTypes)type;
            }

            var timingValue = partRow.QuerySelectorValue(CssSelectors.Parts.Timing);
            if (int.TryParse(timingValue, out var timing))
            {
                part.Timing = (Timings)timing;
            }

            part.Cost = partRow.QuerySelectorValue(CssSelectors.Parts.Cost);
            part.Range = partRow.QuerySelectorValue(CssSelectors.Parts.Range);
            part.Effect = partRow.QuerySelectorValue(CssSelectors.Parts.Effect);
            part.Source = partRow.QuerySelectorValue(CssSelectors.Parts.Source);
            part.IsUsed = partRow.QuerySelectorValue(CssSelectors.Parts.IsUsed) == "1";
            part.IsBroken = partRow.QuerySelectorValue(CssSelectors.Parts.IsBroken) == "1";
        }
    }

    public class Character
    {
        public string Name { get; set; } = string.Empty;
        public int ActionValue { get; set; }

        public List<Part> Parts { get; set; } = [];
        public List<LingeringAttachment> LingeringAttachments { get; set; } = [];
    }

    [DebuggerDisplay("{Category} {Type} {Name} {Timing} {Cost} {Range} {Effect} {Source}")]
    public class Part
    {
        public string Name { get; set; } = string.Empty;
        public PartCategories Category { get; set; }
        public PartTypes Type { get; set; }
        public Timings Timing { get; set; }
        public string Cost { get; set; } = string.Empty;
        public string Range { get; set; } = string.Empty;
        public string Effect { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public bool IsBroken { get; set; }
    }

    public class LingeringAttachment
    {
        public string Target { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int InsanityLevel { get; set; }
        public string InsanityName { get; set; } = string.Empty;
        public string InsanityEffect { get; set; } = string.Empty;
        public string Memo { get; set; } = string.Empty;
    }

    public enum PartCategories
    {
        None = 0,
        /// <summary>
        /// 通常技
        /// </summary>
        Normal = 1,
        /// <summary>
        /// 必殺技
        /// </summary>
        Special,
        /// <summary>
        /// 行動値増加
        /// </summary>
        ActionValueIncrease,
        /// <summary>
        /// 補助
        /// </summary>
        Support,
        /// <summary>
        /// 妨害
        /// </summary>
        Interference,
        /// <summary>
        /// 防御 / 生贄
        /// </summary>
        Defense,
        /// <summary>
        /// 移動
        /// </summary>
        Movement
    }

    public enum PartTypes
    {
        None = 0,
        /// <summary>
        /// ポジション
        /// </summary>
        Position = 1,
        /// <summary>
        /// メインクラス
        /// </summary>
        MainClass = 2,
        /// <summary>
        /// サブクラス
        /// </summary>
        SubClass = 3,
        /// <summary>
        /// 頭
        /// </summary>
        Head = 4,
        /// <summary>
        /// 腕
        /// </summary>
        Arm = 5,
        /// <summary>
        /// 胴
        /// </summary>
        Body = 6,
        /// <summary>
        /// 足
        /// </summary>
        Leg = 7
    }

    public enum Timings
    {
        /// <summary>
        /// オート
        /// </summary>
        Auto = 0,
        /// <summary>
        /// アクション
        /// </summary>
        Action = 1,
        /// <summary>
        /// ジャッジ
        /// </summary>
        Judge = 2,
        /// <summary>
        /// ダメージ
        /// </summary>
        Damage = 3,
        /// <summary>
        /// ラピッド
        /// </summary>
        Rapid = 4
    }
}
