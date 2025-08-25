using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DUANTOTNGHIEP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodTypeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FoodTypeController(ApplicationDbContext context)
        {
            _context = context;
        }
        

        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<FoodType_DTO>>>> GetFoodTypes()
        {
            var foodTypes = await _context.FoodTypes
                .Select(ft => new FoodType_DTO
                {
                    FoodTypeId = ft.FoodTypeId,
                    FoodTypeName = ft.FoodTypeName,
                    Description = ft.Description,
                    CreatedDate = ft.CreatedDate,
                    CreatedBy = ft.CreatedBy,
                    UpdatedDate = ft.UpdatedDate,
                    UpdatedBy = ft.UpdatedBy
                })
                .ToListAsync();

            return Ok(new BaseResponse<IEnumerable<FoodType_DTO>>
            {
                ErrorCode = 0,
                Message = "Lấy danh sách loại món ăn thành công",
                Data = foodTypes
            });
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<FoodType_DTO>>> GetFoodType(Guid id)
        {
            var ft = await _context.FoodTypes.FindAsync(id);
            if (ft == null)
                return NotFound(new BaseResponse<FoodType_DTO>
                {
                    ErrorCode = 1,
                    Message = "Không tìm thấy loại món ăn"
                });

            var dto = new FoodType_DTO
            {
                FoodTypeId = ft.FoodTypeId,
                FoodTypeName = ft.FoodTypeName,
                Description = ft.Description,
                CreatedDate = ft.CreatedDate,
                CreatedBy = ft.CreatedBy,
                UpdatedDate = ft.UpdatedDate,
                UpdatedBy = ft.UpdatedBy
            };

            return Ok(new BaseResponse<FoodType_DTO>
            {
                ErrorCode = 0,
                Message = "Lấy thông tin loại món ăn thành công",
                Data = dto
            });
        }
        [Authorize(Roles = "ADMIN")]

        [HttpPost]
        public async Task<ActionResult<BaseResponse<FoodType_DTO>>> CreateFoodType(CreateFoodType_DTO request)
        {
            var foodType = new FoodType
            {
                FoodTypeId = Guid.NewGuid(),
                FoodTypeName = request.FoodTypeName,
                Description = request.Description,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = "system", // Hoặc lấy từ HttpContext.User.Identity.Name
                UpdatedBy = "system"
            };

            _context.FoodTypes.Add(foodType);
            await _context.SaveChangesAsync();

            var result = new FoodType_DTO
            {
                FoodTypeId = foodType.FoodTypeId,
                FoodTypeName = foodType.FoodTypeName,
                Description = foodType.Description,
                CreatedDate = foodType.CreatedDate,
                CreatedBy = foodType.CreatedBy,
                UpdatedDate = foodType.UpdatedDate,
                UpdatedBy = foodType.UpdatedBy
            };

            return CreatedAtAction(nameof(GetFoodType), new { id = foodType.FoodTypeId }, new BaseResponse<FoodType_DTO>
            {
                ErrorCode = 0,
                Message = "Tạo loại món ăn thành công",
                Data = result
            });
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<FoodType_DTO>>> UpdateFoodType(Guid id, UpdateFoodType_DTO request)
        {
            var foodType = await _context.FoodTypes.FindAsync(id);
            if (foodType == null)
            {
                return NotFound(new BaseResponse<FoodType_DTO>
                {
                    ErrorCode = 1,
                    Message = "Không tìm thấy loại món ăn"
                });
            }

            foodType.FoodTypeName = request.FoodTypeName;
            foodType.Description = request.Description;
            foodType.UpdatedDate = DateTime.UtcNow;
            foodType.UpdatedBy = "system"; // Hoặc từ người dùng hiện tại

            await _context.SaveChangesAsync();

            var dto = new FoodType_DTO
            {
                FoodTypeId = foodType.FoodTypeId,
                FoodTypeName = foodType.FoodTypeName,
                Description = foodType.Description,
                CreatedDate = foodType.CreatedDate,
                CreatedBy = foodType.CreatedBy,
                UpdatedDate = foodType.UpdatedDate,
                UpdatedBy = foodType.UpdatedBy
            };

            return Ok(new BaseResponse<FoodType_DTO>
            {
                ErrorCode = 0,
                Message = "Cập nhật loại món ăn thành công",
                Data = dto
            });
        }
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<object>>> DeleteFoodType(Guid id)
        {
            var foodType = await _context.FoodTypes.FindAsync(id);
            if (foodType == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 1,
                    Message = "Không tìm thấy loại món ăn"
                });
            }

            _context.FoodTypes.Remove(foodType);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 0,
                Message = "Xóa loại món ăn thành công",
                Data = null
            });
        }
    }
}
