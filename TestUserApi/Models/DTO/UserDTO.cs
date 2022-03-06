namespace TestUserApi.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DocTypeId { get; set; } = -1;
        public string DocNumber { get; set; } = string.Empty;


        public UserDTO()
        {
        }



    }
}
