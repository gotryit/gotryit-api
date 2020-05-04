namespace gotryit_api.Repositories
{
    public partial class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public bool Active { get; set; }
        public string PasswordSalt { get; set; }
    }
}
