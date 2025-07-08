namespace DUANTOTNGHIEP.DTOS
{
    public class LoginResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? ProfileImage { get; set; } 
    }
}
