using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.Combo;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DUANTOTNGHIEP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CombosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CombosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task<string?> SaveImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "combos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/combos/{fileName}";
        }

        // GET: api/combos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var combos = await _context.Combos
                .Include(c => c.ComboDetails)
                .ThenInclude(cd => cd.Food)
                .Select(c => new ComboDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Items = c.ComboDetails.Select(cd => new ComboFoodItemDto
                    {
                        FoodId = cd.FoodId,
                        FoodName = cd.Food.Name,
                        Quantity = cd.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new BaseResponse<List<ComboDto>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách combo thành công!",
                Data = combos
            });
        }

        // GET: api/combos/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboDetails)
                .ThenInclude(cd => cd.Food)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
            {
                return NotFound(new BaseResponse<ComboDto>
                {
                    ErrorCode = 404,
                    Message = "Combo không tồn tại!"
                });
            }

            var result = new ComboDto
            {
                Id = combo.Id,
                Name = combo.Name,
                Description = combo.Description,
                Price = combo.Price,
                ImageUrl = combo.ImageUrl,
                Items = combo.ComboDetails.Select(cd => new ComboFoodItemDto
                {
                    FoodId = cd.FoodId,
                    FoodName = cd.Food.Name,
                    Quantity = cd.Quantity
                }).ToList()
            };

            return Ok(new BaseResponse<ComboDto>
            {
                ErrorCode = 200,
                Message = "Lấy thông tin combo thành công!",
                Data = result
            });
        }

        // POST: api/combos
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateComboDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemsJson))
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Combo phải chứa ít nhất 1 món ăn." });

            List<ComboFoodItemCreateDto>? items;
            try
            {
                items = System.Text.Json.JsonSerializer.Deserialize<List<ComboFoodItemCreateDto>>(dto.ItemsJson);
            }
            catch
            {
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Định dạng items không hợp lệ." });
            }

            if (items == null || !items.Any())
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Combo phải chứa ít nhất 1 món ăn." });

            var validFoodIds = await _context.Foods.Select(f => f.Id).ToListAsync();
            if (items.Any(i => !validFoodIds.Contains(i.FoodId)))
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Một hoặc nhiều món ăn không tồn tại." });

            string? imageUrl = null;
            string imagePath = "https://placehold.co/600x400?text=No+Image";

            if (dto.ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(dto.ImageFile.FileName);
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "combos");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                imageUrl = "/uploads/combos/" + fileName;
            }

            var combo = new Combo
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = imageUrl,
                CreatedBy = "System",
                UpdatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            combo.ComboDetails = items.Select(i => new ComboDetail
            {
                Id = Guid.NewGuid(),
                ComboId = combo.Id,
                FoodId = i.FoodId,
                Quantity = i.Quantity,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            }).ToList();

            _context.Combos.Add(combo);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Tạo combo thành công!",
                Data = new { combo.Id }
            });
        }

        // PUT: api/combos/{id}
        [HttpPut("{id}")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateComboDto dto, [FromForm] string? itemsJson, IFormFile? imageFile)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboDetails)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
                return NotFound(new BaseResponse<object> { ErrorCode = 404, Message = "Combo không tồn tại!" });

            if (string.IsNullOrEmpty(itemsJson))
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Combo phải chứa ít nhất 1 món ăn." });

            List<ComboFoodItemUpdateDto>? items;
            try
            {
                items = System.Text.Json.JsonSerializer.Deserialize<List<ComboFoodItemUpdateDto>>(itemsJson);
            }
            catch
            {
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Định dạng JSON không hợp lệ." });
            }

            if (items == null || !items.Any())
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Combo phải chứa ít nhất 1 món ăn." });

            var validFoodIds = await _context.Foods.Select(f => f.Id).ToListAsync();
            if (items.Any(i => !validFoodIds.Contains(i.FoodId)))
                return BadRequest(new BaseResponse<object> { ErrorCode = 400, Message = "Một hoặc nhiều món ăn không tồn tại." });

            if (imageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "combos");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                combo.ImageUrl = "/uploads/combos/" + fileName;
            }

            combo.Name = dto.Name;
            combo.Description = dto.Description;
            combo.Price = dto.Price;
            combo.UpdatedDate = DateTime.UtcNow;
            combo.UpdatedBy = "System";

            var oldDetails = await _context.ComboDetails.Where(cd => cd.ComboId == combo.Id).ToListAsync();
            if (oldDetails.Any())
                _context.ComboDetails.RemoveRange(oldDetails);

            foreach (var i in items)
            {
                _context.ComboDetails.Add(new ComboDetail
                {
                    Id = Guid.NewGuid(),
                    ComboId = combo.Id,
                    FoodId = i.FoodId,
                    Quantity = i.Quantity,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Cập nhật combo thành công!"
            });
        }

        // DELETE: api/combos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboDetails)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Combo không tồn tại!"
                });

            _context.ComboDetails.RemoveRange(combo.ComboDetails);
            _context.Combos.Remove(combo);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Xóa combo thành công!"
            });
        }
    }
}
