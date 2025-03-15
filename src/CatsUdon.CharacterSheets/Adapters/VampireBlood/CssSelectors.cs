namespace CatsUdon.CharacterSheets.Adapters.VampireBlood;

public static class CssSelectors
{
    public const string CharacterName = "#pc_name";
    public const string ActionValue = "#speed_disp > div:nth-child(2) > table:nth-child(1) > tbody:nth-child(1) > tr:nth-child(2) > td:nth-child(2) > input:nth-child(1)";

    public const string PartRows = "#Table_Power > tbody:nth-child(1) > tr";
    public static class Parts
    {
        public const string Name = "td:nth-child(5) > input:nth-child(1)";
        public const string Category = "td:nth-child(3) > input:nth-child(2)";
        public const string Type = "td:nth-child(4) > input:nth-child(2)";
        public const string Timing = "td:nth-child(6) > input:nth-child(2)";
        public const string Cost = "td:nth-child(7) > input:nth-child(1)";
        public const string Range = "td:nth-child(8) > input:nth-child(1)";
        public const string Effect = "td:nth-child(9) > input:nth-child(1)";
        public const string Source = "td:nth-child(10) > input:nth-child(1)";
        public const string IsUsed = "th:nth-child(1) > input:nth-child(1)";
        public const string IsBroken = "th:nth-child(2) > input:nth-child(1)";
    }

    public const string LingeringAttachmentRows = "#Table_roice > tbody:nth-child(1) > tr";
    public static class LingeringAttachment
    {
        public const string Target = "td:nth-child(1) > input:nth-child(1)";
        public const string Type = "td:nth-child(2) > input:nth-child(2)";
        public const string InsanityLevel = "td:nth-child(3) > input:nth-child(2)";
        public const string InsanityName = "td:nth-child(4) > input:nth-child(1)";
        public const string InsanityEffect = "td:nth-child(5) > input:nth-child(1)";
        public const string Memo = "td:nth-child(6) > input:nth-child(1)";

        // td:nth-child(1) > input:nth-child(1)
    }

}
