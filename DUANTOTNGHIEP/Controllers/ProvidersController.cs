using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DUANTOTNGHIEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProvidersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var providers = await _context.Providers.ToListAsync();

            var result = providers.Select(p => new ProviderDTO
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Email = p.Email,
                Phone = p.Phone,
                Description = p.Description
            }).ToList();

            return Ok(new BaseResponse<List<ProviderDTO>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách nhà cung cấp thành công",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy nhà cung cấp",
                    Data = null
                });
            }

            var result = new ProviderDTO
            {
                Id = provider.Id,
                Name = provider.Name,
                Address = provider.Address,
                Email = provider.Email,
                Phone = provider.Phone,
                Description = provider.Description
            };

            return Ok(new BaseResponse<ProviderDTO>
            {
                ErrorCode = 200,
                Message = "Lấy thông tin nhà cung cấp thành công",
                Data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProviderCreateDTO dto)
        {
            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Address = dto.Address,
                Email = dto.Email,
                Phone = dto.Phone,
                Description = dto.Description
            };

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            var result = new ProviderDTO
            {
                Id = provider.Id,
                Name = provider.Name,
                Address = provider.Address,
                Email = provider.Email,
                Phone = provider.Phone,
                Description = provider.Description
            };

            return Ok(new BaseResponse<ProviderDTO>
            {
                ErrorCode = 201,
                Message = "Tạo nhà cung cấp thành công",
                Data = result
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, ProviderUpdateDTO dto)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy nhà cung cấp để cập nhật",
                    Data = null
                });
            }

            provider.Name = dto.Name;
            provider.Address = dto.Address;
            provider.Email = dto.Email;
            provider.Phone = dto.Phone;
            provider.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<string>
            {
                ErrorCode = 200,
                Message = "Cập nhật nhà cung cấp thành công",
                Data = provider.Id.ToString()
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy nhà cung cấp để xóa",
                    Data = null
                });
            }

            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<string>
            {
                ErrorCode = 200,
                Message = "Xóa nhà cung cấp thành công",
                Data = id.ToString()
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Tên không được để trống",
                    Data = null
                });
            }

            var matchedProviders = await _context.Providers
                .Where(p => p.Name.Contains(name))
                .Select(p => new ProviderDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Email = p.Email,
                    Phone = p.Phone,
                    Description = p.Description
                })
                .ToListAsync();

            if (!matchedProviders.Any())
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy nhà cung cấp phù hợp",
                    Data = null
                });
            }

            return Ok(new BaseResponse<List<ProviderDTO>>
            {
                ErrorCode = 200,
                Message = "Tìm thấy nhà cung cấp phù hợp",
                Data = matchedProviders
            });
        }
    }
}
