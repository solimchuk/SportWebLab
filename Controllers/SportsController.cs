using Microsoft.AspNetCore.Authorization; // <-- 1. ДОДАНО ЦЕЙ USING
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models; 
using Microsoft.EntityFrameworkCore; // Потрібен для DbUpdateConcurrencyException

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class SportsController : Controller
    {
        private readonly SportService _sportService;

        public SportsController(SportService sportService)
        {
            _sportService = sportService;
        }

        // GET: Sports
        public async Task<IActionResult> Index()
        {
            var sports = await _sportService.GetSportsAsync();
            return View(sports);
        }

        // GET: Sports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var sport = await _sportService.GetSportByIdAsync(id);
            if (sport == null)
            {
                return NotFound();
            }
            return View(sport);
        }

        // GET: Sports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Sport sport)
        {
            // ModelState валідація залишається тут, бо вона стосується даних з форми
            if (ModelState.IsValid)
            {
                await _sportService.CreateSportAsync(sport);
                return RedirectToAction(nameof(Index));
            }
            return View(sport); // Повертаємо ту саму view з помилками валідації
        }

        // GET: Sports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var sport = await _sportService.GetSportByIdAsync(id);
            if (sport == null)
            {
                return NotFound();
            }
            return View(sport);
        }

        // POST: Sports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Sport sport)
        {
            if (id != sport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _sportService.UpdateSportAsync(sport);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Перевіряємо існування асинхронно
                    if (!await _sportService.SportExistsAsync(sport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Перекидаємо виключення далі, якщо це не помилка "не знайдено"
                    }
                }
                return RedirectToAction(nameof(Index)); // Перенаправляємо на список після успішного оновлення
            }
            return View(sport); // Повертаємо ту саму view з помилками валідації
        }

        // GET: Sports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var sport = await _sportService.GetSportByIdAsync(id);
            if (sport == null)
            {
                return NotFound();
            }
            return View(sport);
        }

        // POST: Sports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _sportService.DeleteSportAsync(id);
            return RedirectToAction(nameof(Index)); // Перенаправляємо на список після видалення
        }

        // Метод SportExistsAsync тепер у сервісі, тому тут він більше не потрібен
    }
}

