using Azure.Core;
using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DUANTOTNGHIEP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Thêm RoleManager
        private readonly IWebHostEnvironment _env;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var usersInUserRole = await _userManager.GetUsersInRoleAsync("Customer");

            var users = usersInUserRole.Select(user => new User_DTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PhoneNumbers = user.PhoneNumbers,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
            }).ToList();

            return Ok(new BaseResponse<List<User_DTO>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách người dùng thành công!",
                Data = users
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Người dùng không tồn tại",
                    Data = null
                });
            }

            // Kiểm tra xem user có thuộc role "Admin" không
            var isAdmin = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isAdmin)
            {
                return Forbid(); // Trả về lỗi 403 nếu user không phải admin
            }

            var userResponse = new User_DTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumbers = user.PhoneNumbers,
                Address = user.Address,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
            };

            return Ok(new BaseResponse<User_DTO>
            {
                ErrorCode = 200,
                Message = "Lấy thông tin người dùng thành công!",
                Data = userResponse
            });
        }

        [HttpPost("register_customer")]
        public async Task<IActionResult> Register([FromForm] UserRegister_DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Email đã tồn tại. Vui lòng sử dụng email khác.",
                    Data = null
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email, // Dùng email làm username
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumbers = request.PhoneNumbers,
                Address = request.Address,
                IsActive = true
            };

            if (request.ProfileImage != null && request.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ProfileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ProfileImage.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối vào DB
                user.ProfileImage = $"/uploads/{uniqueFileName}";
                await _userManager.UpdateAsync(user);
            } else {
                user.ProfileImage = "/uploads/default-avatar.png";
            }

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }

                await _userManager.AddToRoleAsync(user, "Customer");

                //var cart = new Cart
                //{
                //    UserId = user.Id,
                //    CreatedDate = DateTime.UtcNow
                //};

                //_context.Carts.Add(cart);
                //await _context.SaveChangesAsync();

                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Đăng ký User thành công!",
                    Data = user.Id
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Đăng ký thất bại",
                Data = result.Errors
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUser_DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Sid)?.Value; // Lấy ID từ token
            if (userId == null)
            {
                return Unauthorized(new BaseResponse<string>
                {
                    ErrorCode = 401,
                    Message = "Không xác định được người dùng",
                    Data = null
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Người dùng không tồn tại",
                    Data = null
                });
            }

            // Cập nhật thông tin người dùng
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Address = request.Address ?? user.Address;
            user.IsActive = request.IsActive ?? true;
            if (request.PhoneNumbers != null)
            {
                user.PhoneNumbers = request.PhoneNumbers;
            }
            else
            {
                user.PhoneNumbers = user.PhoneNumbers;
            }


            if (request.ProfileImage != null && request.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ProfileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ProfileImage.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối vào DB
                user.ProfileImage = $"/uploads/{uniqueFileName}";
                await _userManager.UpdateAsync(user);
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Cập nhật thông tin thành công!",
                    Data = user.Id
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Cập nhật thất bại",
                Data = result.Errors
            });
        }

        [Authorize(Roles = "ADMIN, STAFF")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Khách hàng không tồn tại",
                    Data = null
                });
            }

            var isStaff = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isStaff)
            {
                return Forbid();
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Khôi phục khách hàng thành công!",
                    Data = user.Id
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Khôi phục thất bại",
                Data = result.Errors
            });
        }

        [Authorize(Roles = "ADMIN, STAFF")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Khách hàng không tồn tại",
                    Data = null
                });
            }

            // Kiểm tra nếu không phải là STAFF thì không cho xóa
            var isStaff = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isStaff)
            {
                return Forbid();
            }

            // Đánh dấu là không còn hoạt động
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Xóa mềm khách hàng thành công!",
                    Data = user.Id
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Xóa mềm thất bại",
                Data = result.Errors
            });
        }
        [Authorize(Roles = "Customer,STAFF")]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword_DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Sid)?.Value; // Lấy ID từ token
            if (userId == null)
            {
                return Unauthorized(new BaseResponse<string>
                {
                    ErrorCode = 401,
                    Message = "Không xác định được người dùng",
                    Data = null
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Người dùng không tồn tại",
                    Data = null
                });
            }

            // Kiểm tra mật khẩu cũ có đúng không
            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!passwordCheck)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Mật khẩu cũ không đúng",
                    Data = null
                });
            }

            // Thực hiện đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Đổi mật khẩu thành công!",
                    Data = null
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Đổi mật khẩu thất bại",
                Data = result.Errors
            });
        }
    }
}
