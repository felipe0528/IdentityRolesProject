using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityRolesProject.Contracts;
using IdentityRolesProject.Data;
using IdentityRolesProject.Models.Users;
using IdentityRolesProject.Models.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdentityRolesProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {

        private readonly IRolesRepository _repo;
        private readonly IMapper _mapper;

        public RolesController(IRolesRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // GET: Roles
        public async Task<ActionResult> Index()
        {
            var list = await _repo.FindAll();
            var model = _mapper.Map<List<IdentityRole>, List<IdentityRoleVM>>(list.ToList());
            return View(model);
        }

        // GET: Roles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var role = await _repo.FindById(id);
            var model = _mapper.Map<IdentityRole, IdentityRoleVM>(role);
            var users = await _repo.FindAllUsersByRole(id);
            if (users.Count > 0)
            {
                model.Users = users.ToList();
            }
            else
            {
                model.Users = new List<ApplicationUser>();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details(IdentityRoleVM rolePar)
        {
            var role = await _repo.FindById(rolePar.Id);
            if (role == null)
            {
                return NotFound();
            }

            var isSuccess = await _repo.AddUserByRole(role.Id, rolePar.NewUserName);
            var users = await _repo.FindAllUsersByRole(rolePar.Id);
            if (users.Count > 0)
            {
                rolePar.Users = users.ToList();
            }
            else
            {
                rolePar.Users = new List<ApplicationUser>();
            }

            if (!isSuccess)
            {
                ModelState.AddModelError(string.Empty, "Could not add " + rolePar.NewUserName);
            }
            return View(rolePar);
        }


        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IdentityRoleVM role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(role);
                }
                var model = new IdentityRole(role.Name);
                var result = await _repo.Create(model);

                if (result != string.Empty)
                {
                    return RedirectToAction(nameof(Details), new { id = result });
                }
            }
            catch (Exception x)
            {
                ModelState.AddModelError(string.Empty, x.InnerException.Message);

            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            var role = await _repo.FindById(id);
            if (role != null)
            {
                var model = _mapper.Map<IdentityRole, IdentityRoleVM>(role);
                return View(model);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IdentityRoleVM role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(role);
                }
                var model = await _repo.FindById(role.Id);
                if (model != null && role != null)
                {
                    model.Name = role.Name;
                }

                var result = await _repo.Update(model);

                if (result != string.Empty)
                {
                    return RedirectToAction(nameof(Details), new { id = result });
                }
            }
            catch (Exception x)
            {
                ModelState.AddModelError(string.Empty, x.InnerException.Message);
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<ActionResult> DeleteAsync(string id)
        {
            var role = await _repo.FindById(id);
            if (role == null)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
            var isSuccess = await _repo.Delete(role);

            return RedirectToAction(nameof(Index));
        }
        // GET: Roles/Delete/5
        public async Task<ActionResult> DeleteUserFromRole(string id, string userId)
        {
            var role = await _repo.FindById(id);
            if (role == null)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
            var isSuccess = await _repo.RemoveUserByRole(id, userId);

            return RedirectToAction(nameof(Details), new { id = id });
        }



    }
}