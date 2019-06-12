using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Client;

namespace TodoListClient.Controllers
{
    public class TodoListController : Controller
    {
        ITokenAcquisition _tokenAcquisition;

        public TodoListController(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        // GET: TodoList
        public ActionResult Index()
        {
            return View();
        }

        // GET: TodoList/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TodoList/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TodoList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TodoList/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TodoList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TodoList/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TodoList/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}