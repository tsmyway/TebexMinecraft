using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace TebexMinecraft.Consumer;

public static class CRUD<T>
{
    public static string Endpoint { get; set; } = string.Empty;

    private static readonly HttpClient _cliente = new HttpClient();

    public static List<T> GetAll()
    {
        var response = _cliente.GetAsync(Endpoint).Result;
        if (response.IsSuccessStatusCode)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
        throw new Exception($"Error en GetAll: {response.StatusCode} - {response.ReasonPhrase}");
    }

    public static T? GetById(object id)
    {
        var response = _cliente.GetAsync($"{Endpoint}/{id}").Result;
        if (response.IsSuccessStatusCode)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(json);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
        throw new Exception($"Error en GetById: {response.StatusCode} - {response.ReasonPhrase}");
    }
    
    public static T? Create(T item)
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(item),
            Encoding.UTF8,
            "application/json");

        var response = _cliente.PostAsync(Endpoint, content).Result;
        var json = response.Content.ReadAsStringAsync().Result;

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        throw new Exception($"Error en Create ({response.StatusCode}): {json}");
    }

    public static bool Update(object id, T item)
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(item),
            Encoding.UTF8,
            "application/json");

        var response = _cliente.PutAsync($"{Endpoint}/{id}", content).Result;
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        throw new Exception($"Error en Update: {response.StatusCode} - {response.ReasonPhrase}");
    }

    public static bool Delete(object id)
    {
        var response = _cliente.DeleteAsync($"{Endpoint}/{id}").Result;
        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        throw new Exception($"Error en Delete: {response.StatusCode} - {response.ReasonPhrase}");
    }
}