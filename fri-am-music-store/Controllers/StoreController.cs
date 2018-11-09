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

        // GET: Store/AddToCart
        public ActionResult AddToCart(int AlbumId)
        {
            // determine CartId to group this user's items together
            string CurrentCartId;
            GetCartId();
            CurrentCartId = Session["CartId"].ToString();

            // create and save a new cart item
            Cart cart = new Cart
            {
                CartId = CurrentCartId,
                AlbumId = AlbumId,
                Count = 1,
                DateCreated = DateTime.Now
            };

            db.Carts.Add(cart);
            db.SaveChanges();

            // load the shopping cart page
            return RedirectToAction("ShoppingCart");
        }

        private void GetCartId()
        {
            // if there is no Cart Id session variable (this is their first item)
            if (Session["CartId"] == null)
            {
                // is the user logged in?
                if (User.Identity.IsAuthenticated)
                {
                    Session["CartId"] = User.Identity.Name;
                }
                else
                {
                    // anonymous so generate new unique string
                    Session["CartId"] = Guid.NewGuid().ToString();
                }
            }
        }

        // GET: Store/ShoppingCart
        public ActionResult ShoppingCart()
        {
            // check or generate Session CartId
            GetCartId();

            // select all the items in the current user's cart
            string CurrentCartId = Session["CartId"].ToString();
            var CartItems = db.Carts.Where(c => c.CartId == CurrentCartId).ToList();

            return View(CartItems);
        }

        // GET: Store/RemoveFromCart/5
        public ActionResult RemoveFromCart(int id)
        {
            // find and delete this record from this user's cart
            Cart CartItem = db.Carts.SingleOrDefault(c => c.RecordId == id);
            db.Carts.Remove(CartItem);
            db.SaveChanges();

            // reload the updated cart page
            return RedirectToAction("ShoppingCart");
        }
    }
}