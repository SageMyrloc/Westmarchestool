namespace Westmarchestool.API.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public bool IsLocked { get; set; }
    }
}