using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers
{
    public class CargoController : BaseController
    {
        private readonly DataContext _context;

        public CargoController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cargos = await _context.Cargos
                .Include(c => c.Documents)
                .ThenInclude(d => d.DocumentType)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cargos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var cargo = await _context.Cargos
                .Include(c => c.Documents)
                .ThenInclude(d => d.DocumentType)
                .Include(c => c.CargoTrackingLogs)
                .ThenInclude(ctl => ctl.UpdatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cargo == null)
            {
                return NotFound();
            }

            return View(cargo);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CargoCompany,ShippingAddress,DeliveryAddress,PackageType,EstimatedDeliveryDate")] Cargo cargo)
        {
            if (ModelState.IsValid)
            {
                cargo.CreatedAt = DateTime.Now;
                cargo.UpdatedAt = DateTime.Now;
                cargo.CargoTrackingNumber = GenerateTrackingNumber();
                
                _context.Add(cargo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cargo);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cargo = await _context.Cargos.FindAsync(id);
            if (cargo == null)
            {
                return NotFound();
            }
            return View(cargo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CargoTrackingNumber,CargoCompany,ShippingAddress,DeliveryAddress,PackageType,ShippingDate,DeliveryDate,EstimatedDeliveryDate,DeliveryStatus,ReceivedBy,DeliveryNotes,IsActive")] Cargo cargo)
        {
            if (id != cargo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    cargo.UpdatedAt = DateTime.Now;
                    _context.Update(cargo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CargoExists(cargo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cargo);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cargo = await _context.Cargos
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cargo == null)
            {
                return NotFound();
            }

            return View(cargo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cargo = await _context.Cargos.FindAsync(id);
            if (cargo != null)
            {
                _context.Cargos.Remove(cargo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string location, string notes)
        {
            var cargo = await _context.Cargos.FindAsync(id);
            if (cargo == null)
            {
                return NotFound();
            }

            var oldStatus = cargo.DeliveryStatus;
            cargo.DeliveryStatus = status;
            cargo.UpdatedAt = DateTime.Now;

            if (status == "DELIVERED")
            {
                cargo.DeliveryDate = DateTime.Now;
            }
            else if (status == "SHIPPED")
            {
                cargo.ShippingDate = DateTime.Now;
            }

            var trackingLog = new CargoTrackingLog
            {
                CargoId = id,
                OldStatus = oldStatus,
                NewStatus = status,
                StatusChangeDate = DateTime.Now,
                Location = location,
                UpdatedBy = GetCurrentUserId(),
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            _context.CargoTrackingLogs.Add(trackingLog);
            _context.Update(cargo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool CargoExists(int id)
        {
            return _context.Cargos.Any(e => e.Id == id);
        }

        private string GenerateTrackingNumber()
        {
            return $"TR{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }
    }
}