using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data; // Потрібен для DbUpdateConcurrencyException
using DataAccess.Models;
using BusinessLogic.Services; // Використовуємо сервіси

namespace WebApplication1.Controllers
{
    public class TeamsController : Controller
    {
        private readonly BusinessLogic.Services.TeamService _teamService;
        private readonly SportService _sportService; // Потрібен для випадаючого списку

        // Просимо (inject) сервіси
        public TeamsController(BusinessLogic.Services.TeamService teamService, SportService sportService)
        {
            _teamService = teamService;
            _sportService = sportService;
        }

        // GET: Teams
        // 1. Оновлюємо сигнатуру
        public async Task<IActionResult> Index()
        {
            // 2. Викликаємо асинхронний метод із сервісу
            var teams = await _teamService.GetTeamsAsync();
            return View(teams);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var team = await _teamService.GetTeamByIdAsync(id); // Використовуємо await
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // GET: Teams/Create
        public async Task<IActionResult> Create()
        {
            // Використовуємо SportService асинхронно
            ViewData["SportId"] = new SelectList(await _sportService.GetSportsAsync(), "Id", "Name");
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SportId")] Team team)
        {
            if (ModelState.IsValid)
            {
                await _teamService.CreateTeamAsync(team); // Використовуємо await
                return RedirectToAction(nameof(Index));
            }
            // Використовуємо SportService асинхронно
            ViewData["SportId"] = new SelectList(await _sportService.GetSportsAsync(), "Id", "Name", team.SportId);
            return View(team);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var team = await _teamService.GetTeamByIdAsync(id); // Використовуємо await
            if (team == null)
            {
                return NotFound();
            }
            ViewData["SportId"] = new SelectList(await _sportService.GetSportsAsync(), "Id", "Name", team.SportId);
            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SportId")] Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.UpdateTeamAsync(team); // Використовуємо await
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Перевіряємо існування асинхронно (сервіс має TeamExistsAsync)
                    if (!await _teamService.TeamExistsAsync(team.Id))
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
            ViewData["SportId"] = new SelectList(await _sportService.GetSportsAsync(), "Id", "Name", team.SportId);
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var team = await _teamService.GetTeamByIdAsync(id); // Використовуємо await
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _teamService.DeleteTeamAsync(id); // Використовуємо await
            return RedirectToAction(nameof(Index));
        }

        // Метод TeamExists тепер у сервісі
    }
}

