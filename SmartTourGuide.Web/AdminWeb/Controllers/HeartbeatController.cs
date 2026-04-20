using AdminWeb.Data;
using AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWeb.Controllers
{
    [ApiController]
    [Route("api/v1/heartbeat")]
    public class HeartbeatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HeartbeatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Ping([FromBody] HeartbeatDto dto)
        {
            if (string.IsNullOrEmpty(dto.DeviceId))
                return BadRequest();

            // Tìm session hiện tại của thiết bị
            var session = await _context.DeviceSessions
                .FirstOrDefaultAsync(s => s.DeviceId == dto.DeviceId);

            if (session == null)
            {
                // Tạo session mới
                session = new DeviceSession
                {
                    DeviceId = dto.DeviceId,
                    DeviceName = dto.DeviceName,
                    LastSeen = DateTime.Now,
                    IsActive = true
                };
                _context.DeviceSessions.Add(session);
            }
            else
            {
                // Cập nhật LastSeen
                session.LastSeen = DateTime.Now;
                session.IsActive = true;
                session.DeviceName = dto.DeviceName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { status = "ok" });
        }

        [HttpPost("offline")]
        public async Task<IActionResult> Offline([FromBody] HeartbeatDto dto)
        {
            if (string.IsNullOrEmpty(dto.DeviceId))
                return BadRequest();

            var session = await _context.DeviceSessions
                .FirstOrDefaultAsync(s => s.DeviceId == dto.DeviceId);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }

    public class HeartbeatDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
    }
}