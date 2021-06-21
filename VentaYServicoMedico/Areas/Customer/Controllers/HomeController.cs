using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VentaYServicoMedico.Data;
using VentaYServicoMedico.Models;
using VentaYServicoMedico.Models.ViewModels;
using VentaYServicoMedico.Utility;

namespace VentaYServicoMedico.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel()
            {
                MenuItem = await
                     _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                Category = await _db.Category.ToListAsync(),
                Coupon = await _db.Coupon.Where(c => c.IsActive == true).ToListAsync()
            };

            //Crea una instancia de notificaciones (reclamos) basándose en User.Identity
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //notificación que especifica el nombre de una entidad
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId
                                 == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            return View(IndexVM);
        }
        [Authorize]
        //[Authorize(Roles = "Manager,Counter,Sales")]
        public async Task<IActionResult> Details(int id)
        {
            var MenuItemFromDb =
              await _db.MenuItem.Include(m => m.Category).Include(
                  m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();
            //if (MenuItemFromDb.Dietary.Equals("2"))
            //{
            //    ViewBag.Dieta = "Dietary";
            //}
            //if (MenuItemFromDb.Dietary.Equals("1"))
            //{
            //    ViewBag.Dieta = "Not Dietary";
            //}
            //if (MenuItemFromDb.Dietary.Equals("3"))
            //{
            //    ViewBag.Dieta = "Very Dietary";
            //}

            string strFecha = MenuItemFromDb.Category.RegisterDate.ToString();
            //"23/05/2021 0:00:00"
            //"2021-05-23"
            strFecha = strFecha.Substring(6, 4) + "-" + strFecha.Substring(3, 2) +
                       "-" + strFecha.Substring(0, 2);
            ViewBag.Fecha = strFecha;
            ShoppingCart carObj = new ShoppingCart()
            {
                MenuItem = MenuItemFromDb,
                MenuItemId = MenuItemFromDb.Id
            };
            return View(carObj);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //obtener todas las notificaciones de autorizacion del usuario actual
                //Name, Role, etc (ClaimsType) desde la app en uso
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                //Una vez obtenido el usuario y sus propiedades, buscamos en la base de datos
                //para verificar la autenticidad y asignar los roles que tiene
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //la propiedad ApplicationUserID se le asignan los valores de claim
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _db.ShoppingCart.Where(c => c.ApplicationUserId
                                          == CartObject.ApplicationUserId
                                          && c.MenuItemId == CartObject.MenuItemId).FirstOrDefaultAsync();
                if (cartFromDb == null)
                {
                    await _db.ShoppingCart.AddAsync(CartObject);
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + CartObject.Count;
                }
                await _db.SaveChangesAsync();
                var count = _db.ShoppingCart.Where(c =>
                              c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);

                return RedirectToAction("Index");
            }
            else
            {
                var MenuItemFromDb = await _db.MenuItem.Include(m =>
                                     m.Category).Include(m => m.SubCategory).Where(m =>
                                     m.Id == CartObject.MenuItemId).FirstOrDefaultAsync();
                //if (MenuItemFromDb.Dietary.Equals("2"))
                //{
                //    ViewBag.Dieta = "Dietary";
                //}
                //if (MenuItemFromDb.Dietary.Equals("1"))
                //{
                //    ViewBag.Dieta = "Not Dietary";
                //}
                //if (MenuItemFromDb.Dietary.Equals("3"))
                //{
                //    ViewBag.Dieta = "Very Dietary";
                //}
                ShoppingCart cartObj = new ShoppingCart()
                {
                    MenuItem = MenuItemFromDb,
                    MenuItemId = MenuItemFromDb.Id
                };
                return View(cartObj);
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
