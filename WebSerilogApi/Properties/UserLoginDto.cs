public class UserLoginDto
{
    public string Username { get; set; }

    [LogIgnore]
    public string Password { get; set; }

    public string IpAddress { get; set; }
}
