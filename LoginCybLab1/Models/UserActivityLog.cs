namespace LoginCybLab1.Models
{
    public class UserActivityLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime ActionTime { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
    }
}

