using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.MVC.Controllers;

public class AccountController : Controller
{
    private const string UserSessionKey = "MinecraftStoreUser";
    private const string CartSessionKey = "MinecraftStoreCart";
    private readonly string _usersApiUrl = "http://localhost:5000/api/Users";
    private static readonly HttpClient _httpClient = new HttpClient();

    // GET: Account/Login
    public ActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(string username, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ModelState.AddModelError("", "Debes ingresar tu nombre de usuario de Minecraft.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        try
        {
            var response = _httpClient.PostAsync($"{_usersApiUrl}/lookup/{username.Trim()}", null).Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<User>(json);

                if (user != null)
                {
                    HttpContext.Session.SetString(UserSessionKey, JsonConvert.SerializeObject(user));
                    TempData["Success"] = $"¡Bienvenido {user.Username}!";
                    
                    PurgeOwnedRanksOnLogin(user.Id);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Ranks");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ModelState.AddModelError("", "El usuario no existe en los servidores oficiales de Mojang.");
            }
            else
            {
                ModelState.AddModelError("", "No se pudo validar el usuario con la API.");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error al conectar con el servidor: {ex.Message}");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Logout()
    {
        HttpContext.Session.Remove(UserSessionKey);
        TempData["Success"] = "Has cerrado sesión correctamente.";
        return RedirectToAction("Index", "Ranks");
    }

    private void PurgeOwnedRanksOnLogin(long userId)
    {
        try
        {
            var ranksResponse = _httpClient.GetAsync($"{_usersApiUrl}/{userId}/ranks").Result;
            if (!ranksResponse.IsSuccessStatusCode) return;

            var ranksJson = ranksResponse.Content.ReadAsStringAsync().Result;
            var ownedRanks = JsonConvert.DeserializeObject<List<string>>(ranksJson) ?? new List<string>();

            string? cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson)) return;

            var cart = JsonConvert.DeserializeObject<List<OrderItem>>(cartJson) ?? new List<OrderItem>();
            int removedCount = cart.RemoveAll(i =>
                i.ProductCategory == "RANK" &&
                ownedRanks.Contains(i.ProductInternalName, StringComparer.OrdinalIgnoreCase));

            if (removedCount > 0)
            {
                HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
                TempData["Warning"] = "Se retiraron de tu carrito los rangos que tu cuenta ya posee.";
            }
        }
        catch
        {
        }
    }
}