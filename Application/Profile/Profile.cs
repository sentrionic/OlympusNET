namespace Application.Profile
{
    public class Profile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public bool Following { get; set; }
        public int Followers { get; set; }
        public int Followee { get; set; }
    }
}