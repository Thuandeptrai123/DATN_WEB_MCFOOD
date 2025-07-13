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
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Thêm RoleManager
        private readonly IWebHostEnvironment _env;

        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employeesInEmployeeRole = await _userManager.GetUsersInRoleAsync("STAFF");

            var employees = employeesInEmployeeRole.Select(user => new User_DTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                PhoneNumbers = user.PhoneNumbers,
                LastName = user.LastName,
                Address = user.Address,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
            }).ToList();

            return Ok(new BaseResponse<List<User_DTO>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách người dùng thành công!",
                Data = employees
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var employee = await _userManager.FindByIdAsync(id);
            if (employee == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Nhân viên không tồn tại",
                    Data = null
                });
            }

            var isSTAFF = await _userManager.IsInRoleAsync(employee, "STAFF");
            if (!isSTAFF)
            {
                return Forbid();
            }

            var userResponse = new User_DTO
            {
                Id = employee.Id,
                UserName = employee.UserName,
                Email = employee.Email,
                FirstName = employee.FirstName,
                PhoneNumbers = employee.PhoneNumbers,
                LastName = employee.LastName,
                Address = employee.Address,
                ProfileImage = employee.ProfileImage,
                IsActive = employee.IsActive,
            };

            return Ok(new BaseResponse<User_DTO>
            {
                ErrorCode = 200,
                Message = "Lấy thông tin nhân viên thành công!",
                Data = userResponse
            });
        }

        [HttpPost]
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
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                PhoneNumbers = request.PhoneNumbers,
                LastName = request.LastName,
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
            }
            else
            {
                user.ProfileImage = "/uploads/default-avatar.png";
            }

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("STAFF"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("STAFF"));
                }

                await _userManager.AddToRoleAsync(user, "STAFF");

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
                    Message = "Đăng ký STAFF thành công!",
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

        [HttpPost("admin-register")]
        public async Task<IActionResult> AdminRegister([FromForm] UserRegister_DTO request)
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
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                PhoneNumbers = request.PhoneNumbers,
                LastName = request.LastName,
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
            }

            else
            {
                user.ProfileImage = "/uploads/default-avatar.png";
            }
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("ADMIN"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("ADMIN"));
                }

                await _userManager.AddToRoleAsync(user, "ADMIN");

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
                    Message = "Đăng ký STAFF thành công!",
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
                    Message = "Nhân viên không tồn tại",
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
            }else
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
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Nhân viên không tồn tại",
                    Data = null
                });
            }

            // Kiểm tra nếu không phải là STAFF thì không cho xóa
            var isStaff = await _userManager.IsInRoleAsync(user, "STAFF");
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
                    Message = "Xóa mềm nhân viên thành công!",
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
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Nhân viên không tồn tại",
                    Data = null
                });
            }

            var isStaff = await _userManager.IsInRoleAsync(user, "STAFF");
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
                    Message = "Khôi phục nhân viên thành công!",
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
        [Authorize(Roles = "ADMIN")]
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
