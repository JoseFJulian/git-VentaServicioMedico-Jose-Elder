using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentaYServicoMedico.Data;
using VentaYServicoMedico.Models;
using VentaYServicoMedico.Utility;

namespace FastFood.Data
{
    public class DbIniatializer : IDbInitiatializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DbIniatializer(ApplicationDbContext db,
                              UserManager<IdentityUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async void Inittialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }
            if (_db.Roles.Any(r => r.Name == SD.ManagerUser)) return;
            _roleManager.CreateAsync(new IdentityRole(SD.ManagerUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.FrontDeskUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.DoctorUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.CustomerEndUser)).GetAwaiter().GetResult();
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                Name = "admin@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "12344444"
            }, "Una#2021").GetAwaiter().GetResult();
            IdentityUser user = await _db.Users.FirstOrDefaultAsync(u => u.Email
                                == "admin@gmail.com");
            await _userManager.AddToRoleAsync(user, SD.ManagerUser);
        }
    }
}
