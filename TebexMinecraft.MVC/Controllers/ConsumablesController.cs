using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TebexMinecraft.Consumer;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.MVC.Controllers;

public class ConsumablesController : Controller
{
    public ConsumablesController()
    {
        CRUD<Consumable>.Endpoint = "http://localhost:5000/api/Consumables";
    }

    public ActionResult Index()
    {
        try
        {
            List<Consumable> lista = CRUD<Consumable>.GetAll() ?? new List<Consumable>();
            return View(lista);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al obtener consumibles: {ex.Message}");
            return View(new List<Consumable>());
        }
    }

    public ActionResult Details(int id)
    {
        var modelo = CRUD<Consumable>.GetById(id);
        if (modelo == null) return NotFound();
        return View(modelo);
    }

    public ActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Consumable modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Consumable>.Create(modelo);
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
        var modelo = CRUD<Consumable>.GetById(id);
        if (modelo == null) return NotFound();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Consumable modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Consumable>.Update(id, modelo);
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
        var modelo = CRUD<Consumable>.GetById(id);
        if (modelo == null) return NotFound();
        return View(modelo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        try
        {
            CRUD<Consumable>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}