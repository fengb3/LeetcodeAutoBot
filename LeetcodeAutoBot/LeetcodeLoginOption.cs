namespace LeetcodeAutoBot;

public class LeetcodeLoginOption
{
    [ConfigurationKeyName("USERNAME")]
    public string Username { get; set; } = string.Empty;
    
    [ConfigurationKeyName("PASSWORD")]
    public string Password { get; set; } = string.Empty;
}