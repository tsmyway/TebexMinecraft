using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TebexMinecraft.Consumer;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.MVC.Controllers;

public class PrefixesController : Controller
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _usersApiUrl = "http://localhost:5000/api/Users";

    public PrefixesController()
    {
        CRUD<Prefix>.Endpoint = "http://localhost:5000/api/Prefixes";
    }

    public ActionResult Index()
    {
        try
        {
            List<Prefix> lista = CRUD<Prefix>.GetAll() ?? new List<Prefix>();
            
            var ownedPrefixes = new List<string>();

            var userJson = HttpContext.Session.GetString("MinecraftStoreUser");
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<User>(userJson);
                if (user != null)
                {
                    try
                    {
                        var response = _httpClient.GetAsync($"{_usersApiUrl}/{user.Id}/prefixes").Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var json = response.Content.ReadAsStringAsync().Result;
                            ownedPrefixes = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                        }
                    }
                    catch
                    {
                        ownedPrefixes = new List<string>();
                    }
                }
            }

            ViewBag.OwnedPrefixes = ownedPrefixes;
            return View(lista);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al obtener prefijos: {ex.Message}");
            ViewBag.OwnedPrefixes = new List<string>();
            return View(new List<Prefix>());
        }
    }

    public ActionResult Details(int id)
    {
        try
        {
            var modelo = CRUD<Prefix>.GetById(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }
        catch
        {
            return NotFound();
        }
    }

    public ActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Prefix modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Prefix>.Create(modelo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
            }
        }
        return View(modelo);
    }

    public ActionResult Edit(int id)
    {
        try
        {
            var modelo = CRUD<Prefix>.GetById(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Prefix modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Prefix>.Update(id, modelo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
            }
        }
        return View(modelo);
    }

    public ActionResult Delete(int id)
    {
        try
        {
            var modelo = CRUD<Prefix>.GetById(id);
            if (modelo == null) return NotFound();
            return View(modelo);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        try
        {
            CRUD<Prefix>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}