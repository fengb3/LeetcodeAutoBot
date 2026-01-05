namespace LeetcodeAutoBot.Database.Models;

public class LocalStorageEntry
{
    public int AccountId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
