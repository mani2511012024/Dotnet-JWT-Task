namespace userRegistration
{
    public class Register
    {
        public string email { get; set; }
        public byte[] passwordHash { get; set; }
        public byte[] passwordSalt { get; set; }
    }
}