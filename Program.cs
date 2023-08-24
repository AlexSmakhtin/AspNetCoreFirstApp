using Microsoft.AspNetCore.Mvc;
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
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<ICatalog<Item>, ThreadSafeCatalog>();
            builder.Services.AddSingleton<IDateViewer, UTCDateTimeViewer>();
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            //DateTime
            app.MapGet("/local_time", async (IDateViewer dateViewer) =>
            {
                return dateViewer.DateTimeNow("K yyyy.MM.dd dddd HH:mm:ss");
            });//local dateTime
            //1.1 add item
            app.MapPost("/items", async ([FromBody] Item item,
                [FromServices] ICatalog<Item> catalog, HttpResponse response) =>
            {
                try
                {
                    catalog.AddItem(item);
                    response.StatusCode = StatusCodes.Status201Created;
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }); //REST
            app.MapPost("/add_item", async ([FromBody] Item item,
                [FromServices] ICatalog<Item> catalog, HttpResponse response) =>
            {
                try
                {
                    catalog.AddItem(item);
                    response.StatusCode = StatusCodes.Status201Created;
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }); //RPC
            //1.2 get items
            app.MapGet("/items", async ([FromServices] ICatalog<Item> catalog) =>
            {
                return catalog.GetItems();
            }); //REST
            app.MapGet("/items/{index:int}", async (int index, ICatalog<Item> catalog,
                HttpResponse response) =>
            {
                try
                {
                    return catalog.GetItemByIndex(index);
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
                return "Bad request";
            });//REST by index
            app.MapGet("/get_items", async ([FromServices] ICatalog<Item> catalog) =>
            {
                return catalog.GetItems();
            }); //RPC
            app.MapGet("/get_item", async ([FromQuery] int index, ICatalog<Item> catalog,
                HttpResponse response) =>
            {
                try
                {
                    return catalog.GetItemByIndex(index);
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
                return "Bad request";
            }); //RPC by index
            //1.3 delete item
            app.MapDelete("/items/{index:int}", async (int index, ICatalog<Item> catalog,
                HttpResponse response) =>
            {
                try
                {
                    catalog.DeleteItem(index);
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }); //REST
            app.MapPost("/delete_item", async ([FromQuery] int index, ICatalog<Item> catalog,
                HttpResponse response) =>
            {
                try
                {
                    catalog.DeleteItem(index);
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }); //RPC
            //1.4 update item
            app.MapPut("/items/{index:int}", async (int index, ICatalog<Item> catalog,
                HttpResponse response, Item item) =>
            {
                try
                {
                    catalog.UpdateItem(index, item);
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }); //REST
            app.MapPost("/update_item", async ([FromQuery] int index, ICatalog<Item> catalog,
                HttpResponse response, Item item) =>
            {
                try
                {
                    catalog.UpdateItem(index, item);
                }
                catch (Exception)
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }); //RPC
            //2 clear 
            app.MapPost("/clear", async (ICatalog<Item> catalog) =>
            {
                catalog.Clear();
            });
            app.Run();
        }

    }
}
