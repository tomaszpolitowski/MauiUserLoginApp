namespace UserApi.Models
{
    public record RegisterRequest(string Email, string Password, string ConfirmPassword, string FirstName, string LastName);
    public record LoginRequest(string Email, string Password);
    public record UserDto(int Id, string Email, string FirstName, string LastName, string CreatedAt);
}