using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TebexMinecraft.Consumer;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.MVC.Controllers;

public class CartController : Controller
{
    private const string CartSessionKey = "MinecraftStoreCart";
    private const string UserSessionKey = "MinecraftStoreUser";
    private readonly string _ordersApiUrl = "http://localhost:5000/api/Orders";
    private readonly string _usersApiUrl = "http://localhost:5000/api/Users";
    private static readonly HttpClient _httpClient = new HttpClient();

    public ActionResult Index()
    {
        PurgeOwnedItemsFromCart();
        var cart = GetCartFromSession();
        return View(cart);
    }

    // POST: Cart/Add
    [HttpPost]
    public ActionResult Add(OrderItem item)
    {
        if (item.Quantity <= 0) item.Quantity = 1;

        var cart = GetCartFromSession();
        
        string? userJson = HttpContext.Session.GetString(UserSessionKey);
        if (!string.IsNullOrEmpty(userJson))
        {
            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user != null)
            {
                if (item.ProductCategory == "RANK" && UserAlreadyOwnsRank(user.Id, item.ProductInternalName))
                {
                    TempData["Error"] = $"Tu cuenta ya posee el rango {item.ProductName}.";
                    return RedirectBackOrDefault();
                }
                
                if (item.ProductCategory == "PREFIX" && UserAlreadyOwnsPrefix(user.Id, item.ProductInternalName))
                {
                    TempData["Error"] = $"Tu cuenta ya posee el tag/prefijo {item.ProductName}.";
                    return RedirectBackOrDefault();
                }
            }
        }

        var existing = cart.FirstOrDefault(i => 
            i.ProductCategory == item.ProductCategory && 
            i.ProductInternalName == item.ProductInternalName);

        if (existing != null)
        {
            if (item.ProductCategory == "RANK" || item.ProductCategory == "PREFIX")
            {
                TempData["Error"] = $"El artículo '{item.ProductName}' ya está agregado en tu carrito.";
                return RedirectBackOrDefault();
            }

            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }

        SaveCartToSession(cart);
        TempData["Success"] = $"¡{item.ProductName} añadido al carrito!";

        return RedirectBackOrDefault();
    }

    // POST: Cart/Remove
    [HttpPost]
    public ActionResult Remove(int index)
    {
        var cart = GetCartFromSession();
        if (index >= 0 && index < cart.Count)
        {
            cart.RemoveAt(index);
            SaveCartToSession(cart);
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Cart/Checkout
    [HttpPost]
    public ActionResult Checkout()
    {
        PurgeOwnedItemsFromCart();
        var cart = GetCartFromSession();

        if (!cart.Any())
        {
            TempData["Error"] = "Tu carrito está vacío o los productos ya pertenecían a tu cuenta.";
            return RedirectToAction(nameof(Index));
        }

        string? userJson = HttpContext.Session.GetString(UserSessionKey);
        if (string.IsNullOrEmpty(userJson))
        {
            TempData["Error"] = "Debes ingresar tu nick de Minecraft antes de pagar.";
            return RedirectToAction("Login", "Account");
        }

        var user = JsonConvert.DeserializeObject<User>(userJson);

        var order = new Order
        {
            UserId = user!.Id,
            OrderCode = "PENDING",
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            TotalAmount = cart.Sum(i => i.UnitPrice * i.Quantity),
            Items = cart
        };

        try
        {
            CRUD<Order>.Endpoint = _ordersApiUrl;
            var createdOrder = CRUD<Order>.Create(order);

            if (createdOrder == null)
            {
                TempData["Error"] = "No se pudo procesar la orden.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction("Success", new { orderCode = createdOrder.OrderCode });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar la compra: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    public ActionResult Success(string orderCode)
    {
        ViewBag.OrderCode = orderCode;
        return View();
    }

    // POST: Cart/SimulatePayment
    [HttpPost]
    public ActionResult SimulatePayment(string orderCode)
    {
        try
        {
            var response = _httpClient.PostAsync($"{_ordersApiUrl}/simulate-payment/{orderCode}", null).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "¡Pago acreditado! Tus beneficios ya están disponibles en el servidor.";
            }
            else
            {
                TempData["Error"] = "No se pudo procesar el pago.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al conectar con la API: {ex.Message}";
        }

        return RedirectToAction("Success", new { orderCode });
    }

    private void PurgeOwnedItemsFromCart()
    {
        string? userJson = HttpContext.Session.GetString(UserSessionKey);
        if (string.IsNullOrEmpty(userJson)) return;

        var user = JsonConvert.DeserializeObject<User>(userJson);
        if (user == null) return;

        try
        {
            var cart = GetCartFromSession();
            bool cartModified = false;
            
            var ranksResponse = _httpClient.GetAsync($"{_usersApiUrl}/{user.Id}/ranks").Result;
            if (ranksResponse.IsSuccessStatusCode)
            {
                var ranksJson = ranksResponse.Content.ReadAsStringAsync().Result;
                var ownedRanks = JsonConvert.DeserializeObject<List<string>>(ranksJson) ?? new List<string>();

                int ranksRemoved = cart.RemoveAll(i =>
                    i.ProductCategory == "RANK" &&
                    ownedRanks.Contains(i.ProductInternalName, StringComparer.OrdinalIgnoreCase));

                if (ranksRemoved > 0) cartModified = true;
            }
            
            var prefixesResponse = _httpClient.GetAsync($"{_usersApiUrl}/{user.Id}/prefixes").Result;
            if (prefixesResponse.IsSuccessStatusCode)
            {
                var prefixesJson = prefixesResponse.Content.ReadAsStringAsync().Result;
                var ownedPrefixes = JsonConvert.DeserializeObject<List<string>>(prefixesJson) ?? new List<string>();

                int prefixesRemoved = cart.RemoveAll(i =>
                    i.ProductCategory == "PREFIX" &&
                    ownedPrefixes.Contains(i.ProductInternalName, StringComparer.OrdinalIgnoreCase));

                if (prefixesRemoved > 0) cartModified = true;
            }

            if (cartModified)
            {
                SaveCartToSession(cart);
                TempData["Warning"] = "Se retiraron de tu carrito los rangos o tags que tu cuenta ya posee.";
            }
        }
        catch
        {
        }
    }

    private bool UserAlreadyOwnsRank(long userId, string serverRank)
    {
        try
        {
            var response = _httpClient.GetAsync($"{_usersApiUrl}/{userId}/ranks").Result;
            if (!response.IsSuccessStatusCode) return false;

            var json = response.Content.ReadAsStringAsync().Result;
            var ownedRanks = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

            return ownedRanks.Contains(serverRank, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private bool UserAlreadyOwnsPrefix(long userId, string serverTag)
    {
        try
        {
            var response = _httpClient.GetAsync($"{_usersApiUrl}/{userId}/prefixes").Result;
            if (!response.IsSuccessStatusCode) return false;

            var json = response.Content.ReadAsStringAsync().Result;
            var ownedPrefixes = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();

            return ownedPrefixes.Contains(serverTag, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private ActionResult RedirectBackOrDefault()
    {
        string? returnUrl = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Ranks");
    }

    private List<OrderItem> GetCartFromSession()
    {
        string? data = HttpContext.Session.GetString(CartSessionKey);
        return string.IsNullOrEmpty(data)
            ? new List<OrderItem>()
            : JsonConvert.DeserializeObject<List<OrderItem>>(data) ?? new List<OrderItem>();
    }

    private void SaveCartToSession(List<OrderItem> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
    }
}