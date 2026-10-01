using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TebexMinecraft.Consumer;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.MVC.Controllers;

public class AppealsController : Controller
{
    public AppealsController()
    {
        CRUD<Appeal>.Endpoint = "http://localhost:5000/api/Appeals";
    }

    public ActionResult Index()
    {
        try
        {
            List<Appeal> lista = CRUD<Appeal>.GetAll() ?? new List<Appeal>();
            return View(lista);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al obtener apelaciones: {ex.Message}");
            return View(new List<Appeal>());
        }
    }

    public ActionResult Details(int id)
    {
        var modelo = CRUD<Appeal>.GetById(id);
        if (modelo == null) return NotFound();
        return View(modelo);
    }

    public ActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(Appeal modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Appeal>.Create(modelo);
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
        var modelo = CRUD<Appeal>.GetById(id);
        if (modelo == null) return NotFound();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, Appeal modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                CRUD<Appeal>.Update(id, modelo);
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
        var modelo = CRUD<Appeal>.GetById(id);
        if (modelo == null) return NotFound();
        return View(modelo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        try
        {
            CRUD<Appeal>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}