using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS;
using DUANTOTNGHIEP.DTOS.BaseResponses;

using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DUANTOTNGHIEP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }
        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] UserRegister_DTO dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var existingUserByUsername = await _userManager.FindByNameAsync(dto.UserName);
        //    if (existingUserByUsername != null)
        //        return BadRequest(new { message = "Tên người dùng đã tồn tại." });

        //    var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
        //    if (existingUserByEmail != null)
        //        return BadRequest(new { message = "Email đã được sử dụng." });

        //    var newUser = new ApplicationUser
        //    {
        //        UserName = dto.UserName,
        //        Email = dto.Email,
        //        FirstName = dto.FirstName,
        //        LastName = dto.LastName,
        //        Address = dto.Address,
        //        IsEmployee = false, // Mặc định là khách hàng
        //        EmailConfirmed = true // bật xác thực email hay không
        //    };

        //    var result = await _userManager.CreateAsync(newUser, dto.Password);

        //    if (result.Succeeded)
        //    {
        //        // mặc định role "Customer"
        //        await _userManager.AddToRoleAsync(newUser, "Customer");
        //        return Ok(new { message = "Đăng ký thành công." });
        //    }


        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(error.Code, error.Description);
        //    }

        //    return BadRequest(ModelState);
        //}
        [HttpPost("login-admin")]
        public async Task<IActionResult> Login([FromBody] Login_DTO request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // ✅ Kiểm tra xem có phải role admin hoặc nhân viên
                    if (!roles.Contains("ADMIN") && !roles.Contains("STAFF"))
                    {
                        return Unauthorized(new BaseResponse<string>
                        {
                            ErrorCode = 403,
                            Message = "Bạn không có quyền đăng nhập vào hệ thống quản trị."
                        });
                    }

                    var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                    foreach (var role in roles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var token = GenerateJwtToken(authClaims);
                    var loginInfo = new LoginResponseDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        LastName = user.LastName,
                        FirstName = user.FirstName,
                        ProfileImage = user.ProfileImage,
                        Token = token,
                    };

                    return Ok(new BaseResponse<LoginResponseDTO> { Data = loginInfo });
                }
            }

            return Unauthorized(new BaseResponse<string>
            {
                ErrorCode = 401,
                Message = "Tài khoản hoặc mật khẩu không đúng."
            });
        }


        [HttpPost("login-customer")]
        public async Task<IActionResult> LoginCustomer([FromBody] Login_DTO request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // ✅ Chỉ cho phép đăng nhập nếu có role là "Customer"
                    if (!roles.Contains("Customer"))
                    {
                        return Unauthorized(new BaseResponse<string>
                        {
                            ErrorCode = 403,
                            Message = "Bạn không có quyền đăng nhập vào khu vực khách hàng."
                        });
                    }

                    // ✅ Nếu tài khoản bị khóa
                    if (!user.IsActive)
                    {
                        return Unauthorized(new BaseResponse<string>
                        {
                            ErrorCode = 403,
                            Message = "Tài khoản của bạn đã bị khóa."
                        });
                    }

                    var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                    foreach (var role in roles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var token = GenerateJwtToken(authClaims);
                    var loginInfo = new LoginResponseDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        LastName = user.LastName,
                        FirstName = user.FirstName,
                        ProfileImage = user.ProfileImage,
                        Token = token,
                    };

                    return Ok(new BaseResponse<LoginResponseDTO> { Data = loginInfo });
                }
            }

            return Unauthorized(new BaseResponse<string>
            {
                ErrorCode = 401,
                Message = "Tài khoản hoặc mật khẩu không đúng."
            });
        }



        private string GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLogin_DTO dto)
        {
            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest("Email không hợp lệ.");

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                var (firstName, lastName) = SplitFullName(dto.FullName);

                user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = firstName,
                    LastName = lastName,
                    Address = "",
                    ProfileImage = "",
                    IsEmployee = false,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }


            var roles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GenerateJwtToken(authClaims);
            var loginInfo = new LoginResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token
            };

            return Ok(new BaseResponse<LoginResponseDTO> { Data = loginInfo });
        }
        // tách chuổi dành riêng cho đăng nhập gg
        private (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return ("", "");

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return (parts[0], "");

            var lastName = parts.Last();
            var firstName = string.Join(" ", parts.Take(parts.Length - 1));

            return (firstName, lastName);
        }

    }
}
