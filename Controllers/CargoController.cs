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


        public IActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CargoTrackingNumber,CargoCompany,ShippingAddress,DeliveryAddress,PackageType,EstimatedDeliveryDate")] Cargo cargo, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(cargo.CargoTrackingNumber))
            {
                ModelState.AddModelError("CargoTrackingNumber", "Takip numarası zorunludur.");
            }
            else
            {
                // Check if tracking number already exists
                var existingCargo = await _context.Cargos
                    .FirstOrDefaultAsync(c => c.CargoTrackingNumber == cargo.CargoTrackingNumber);
                if (existingCargo != null)
                {
                    ModelState.AddModelError("CargoTrackingNumber", "Bu takip numarası zaten kullanılmaktadır.");
                }
            }

            if (ModelState.IsValid)
            {
                cargo.CreatedAt = DateTime.Now;
                cargo.UpdatedAt = DateTime.Now;
                
                _context.Add(cargo);
                await _context.SaveChangesAsync();
                
                // If returnUrl is provided, redirect back to Document/Create with the new cargo ID
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    TempData["Success"] = "Kargo başarıyla oluşturuldu ve seçildi!";
                    TempData["NewCargoId"] = cargo.Id; // Pass the new cargo ID back
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View(cargo);
        }



    }
}