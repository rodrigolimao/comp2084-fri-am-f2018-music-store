using fri_am_music_store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace fri_am_music_store.Controllers
{
    public class AlbumsController : Controller
    {
        // GET: Albums
        public ActionResult index()
        {
            var albums = new List<Album>();

            // make a mock list of 10 albums
            for (int i = 1; i <= 10; i++)
            {
                albums.Add(new Album { Title = "Album " + i.ToString() });
            }

            // pass the mock data to view - 1st version
            //ViewBag.albums = albums;

            // pass the strongly-typed album list directly to the view
            return View(albums);
        }
    }
}