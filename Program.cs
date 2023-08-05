using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AspNetCoreFirstApp
{
    public class Program
    {
        static List<Item> items = new List<Item>();
        static object lockObject = new object();
        public static void Main(string[] args)
        {
            items.Add(new Item("Item 1", 1.99m));
            items.Add(new Item("Item 2", 2.99m));
            items.Add(new Item("Item 3", 3.99m));
            items.Add(new Item("Item 4", 4.99m));
            items.Add(new Item("Item 5", 5.99m));
            items.Add(new Item("Item 6", 6.99m));
            items.Add(new Item("Item 7", 7.99m));
            items.Add(new Item("Item 8", 8.99m));

            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            //1.1 add item
            app.MapPost("/items", AddItem); //REST
            app.MapPost("/add_item", AddItem); //RPC
            //1.2 get items
            app.MapGet("/items", GetItems); //REST
            app.MapGet("/items/{index}", GetItemByIndex);//REST by index
            app.MapGet("/get_items", GetItems); //RPC
            app.MapGet("/get_item", GetItemByIndex); //RPC by index
            //1.3 delete item
            app.MapDelete("/items/{index}", DeleteItem); //REST
            app.MapPost("/delete_item", DeleteItem); //RPC
            //1.4 update item
            app.MapPut("/items/{index}", UpdateItem); //REST
            app.MapPost("/update_item", UpdateItem); //RPC
            //2 clear 
            app.MapPost("/clear", ClearTheList);
            app.Run();
        }

        static void ClearTheList(HttpContext context)
        {
            lock (lockObject)
            {
                items.Clear();
            }
            return;
        }

        static async Task UpdateItem(HttpContext context, int index)
        {
            if (context.Request.HasJsonContentType())
            {
                try
                {
                    var item = await context.Request.ReadFromJsonAsync<Item>();
                    if (item != null &&
                        item.Name != string.Empty &&
                        item.Cost != 0)
                    {
                        lock (lockObject)
                        {
                            items[index] = item;
                        }
                        context.Response.StatusCode = StatusCodes.Status201Created;
                        return;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
            }
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        static void DeleteItem(HttpContext context, int index)
        {
            try
            {
                lock (lockObject)
                {
                    items.RemoveAt(index);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        static string GetItemByIndex(HttpContext context, int index)
        {
            try
            {
                lock (lockObject)
                {
                    return items[index].ToString();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return $"Item with index \"{index}\" not found";
            }
        }

        static async Task AddItem(HttpContext context)
        {
            if (context.Request.HasJsonContentType())
            {
                try
                {
                    var item = await context.Request.ReadFromJsonAsync<Item>();
                    if (item != null &&
                        item.Name != string.Empty &&
                        item.Cost != 0)
                    {
                        lock (lockObject)
                        {
                            items.Add(item);
                        }
                        context.Response.StatusCode = StatusCodes.Status201Created;
                        return;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
            }
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        static string GetItems()
        {
            var stringBuilder = new StringBuilder();
            lock (lockObject)
            {
                foreach (var item in items)
                {
                    stringBuilder.Append(item.ToString());
                    stringBuilder.Append('\n');
                }
            }
            return stringBuilder.ToString();
        }

    }
}
