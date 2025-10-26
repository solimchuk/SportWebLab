using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data; // for DbUpdateConcurrencyException if needed
using DataAccess.Models;
using BusinessLogic.Services;

namespace WebApplication1.Controllers
{
    public class PlayersController : Controller
    {
        private readonly PlayerService _playerService;
        private readonly TeamService _teamService;

        public PlayersController(PlayerService playerService, TeamService teamService)
        {
            _playerService = playerService;
            _teamService = teamService;
        }

        // GET: Players
        public async Task<IActionResult> Index(string? searchString)
        {
            var players = await _playerService.GetPlayersAsync(searchString);
            return View(players);
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null) return NotFound();
            return View(player);
        }

        // GET: Players/Create
        public async Task<IActionResult> Create()
        {
            ViewData["TeamId"] = new SelectList(await _teamService.GetTeamsAsync(), "Id", "Name");
            return View();
        }

        // POST: Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Number,TeamId")] Player player)
        {
            if (ModelState.IsValid)
            {
                await _playerService.CreatePlayerAsync(player);
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(await _teamService.GetTeamsAsync(), "Id", "Name", player.TeamId);
            return View(player);
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null) return NotFound();
            ViewData["TeamId"] = new SelectList(await _teamService.GetTeamsAsync(), "Id", "Name", player.TeamId);
            return View(player);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Number,TeamId")] Player player)
        {
            if (id != player.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _playerService.UpdatePlayerAsync(player);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _playerService.PlayerExistsAsync(player.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(await _teamService.GetTeamsAsync(), "Id", "Name", player.TeamId);
            return View(player);
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null) return NotFound();
            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _playerService.DeletePlayerAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}