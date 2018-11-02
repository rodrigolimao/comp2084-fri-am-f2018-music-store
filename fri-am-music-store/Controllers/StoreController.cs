using fri_am_music_store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fri_am_music_store.Controllers
{
    public class StoreController : Controller
    {
        // add db connection
        private MusicStoreModel db = new MusicStoreModel();

        // GET: Store
        public ActionResult Index()
        {
            // get genre list to display on main store page
            var genres = db.Genres.OrderBy(g => g.Name).ToList();
            return View(genres);
        }

        // GET: Store/Albums?genre=Name
        public ActionResult Albums(string genre)
        {
            // get albums for selected genre
            var albums = db.Albums.Where(a => a.Genre.Name == genre).OrderBy(a => a.Title).ToList();

            ViewBag.genre = genre;

            // show the view and pass the list of albums to it
            return View(albums);
        }

        // GET: Store/Product/Product-Name
        public ActionResult Product(string ProductName)
        {
            ViewBag.Product = ProductName;
            return View();
        }
    }
}