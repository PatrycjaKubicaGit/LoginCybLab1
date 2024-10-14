using System.ComponentModel.DataAnnotations;

namespace LoginCybLab1.Views.ViewModels
{
    public class UserViewModel
    {
        [Key]
        public string Id { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
