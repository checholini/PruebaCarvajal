using System.ComponentModel.DataAnnotations;

namespace UserAuth.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DocTypeId { get; set; }
        public string DocNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
