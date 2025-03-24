namespace CatsUdon.CharacterSheets.Web.Infrastructure;

public class CommitHashProvider
{
    private static readonly Lazy<string> commitHash = new(ReadCommitHash);

    public static string CommitHash => commitHash.Value;

    private static string ReadCommitHash()
    {
        if (!File.Exists("commit_hash"))
            return "No Info";

        var hash = File.ReadAllText("commit_hash");
        return hash switch
        {
            { Length: > 8 } => hash[0..8],
            _ => hash
        };
    }
}
