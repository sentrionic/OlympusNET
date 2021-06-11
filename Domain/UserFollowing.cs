namespace Domain
{
    public class UserFollowing
    {
        public int ObserverId { get; set; }
        public User Observer { get; set; }
        public int TargetId { get; set; }
        public User Target { get; set; }
    }
}