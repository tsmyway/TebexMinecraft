using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TebexMinecraft.Consumer;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.MVC.Controllers;

public class RanksController : Controller
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public RanksController()
    {
        CRUD<Rank>.Endpoint = "http://localhost:5000/api/Ranks";
    }

    // GET: Ranks
    public ActionResult Index()
    {
        try
        {
            List<Rank> lista = CRUD<Rank>.GetAll() ?? new List<Rank>();
            
            var ownedRanks = new List<string>();
            
            var userJson = HttpContext.Session.GetString("MinecraftStoreUser");
            
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<User>(userJson);
                if (user != null)
                {
                    try
                    {
                        var response = _httpClient.GetAsync($"http://localhost:5000/api/Users/{user.Id}/ranks").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var json = response.Content.ReadAsStringAsync().Result;
                            ownedRanks = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                        }
                    }
                    catch
                    {
                        ownedRanks = new List<string>();
                    }
                }
            }

            ViewBag.OwnedRanks = ownedRanks;
            return View(lista);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al obtener el listado: {ex.Message}");
            ViewBag.OwnedRanks = new List<string>();
            return View(new List<Rank>());
        }
    }

    // GET: Ranks/Details/5
    public ActionResult Details(int id)
    {
        try
        {
            Rank? modelo = CRUD<Rank>.GetById(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }
        catch
        {
            return NotFound();
        }
    }

    // GET: Ranks/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: Ranks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Rank modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Rank>.Create(modelo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
            }
        }

        return View(modelo);
    }

    // GET: Ranks/Edit/5
    public ActionResult Edit(int id)
    {
        try
        {
            Rank? modelo = CRUD<Rank>.GetById(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }
        catch
        {
            return NotFound();
        }
    }

    // POST: Ranks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Rank modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Rank>.Update(id, modelo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
            }
        }

        return View(modelo);
    }

    // GET: Ranks/Delete/5
    public ActionResult Delete(int id)
    {
        try
        {
            Rank? modelo = CRUD<Rank>.GetById(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }
        catch
        {
            return NotFound();
        }
    }

    // POST: Ranks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        try
        {
            CRUD<Rank>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}