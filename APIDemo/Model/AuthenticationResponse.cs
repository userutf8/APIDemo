namespace APIDemo.Model
{
    public class AuthenticationResponse // represents response
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}