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

            // if item is already in the user's cart, increment count by 1
            Cart cart = db.Carts.SingleOrDefault(c => c.AlbumId == AlbumId &&
                c.CartId == CurrentCartId);

            if (cart == null)
            {
                // create and save a new cart item
                cart = new Cart
                {
                    CartId = CurrentCartId,
                    AlbumId = AlbumId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };

                db.Carts.Add(cart);
            }
            else
            {
                // increment the count by 1
                cart.Count++;
            }

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

        // GET: Store/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            // migrate the cart to the username if shopping anonymously
            MigrateCart();
            return View();
        }

        // POST: Store/Checkout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(FormCollection values)
        {
            // create a new order & populate it from the form values
            Order order = new Order();
            TryUpdateModel(order);

            // autopopulate the fields that we can
            order.Username = User.Identity.Name;
            order.Email = User.Identity.Name;
            order.OrderDate = DateTime.Now;

            // get user's items and calc cart total
            var CartItems = db.Carts.Where(c => c.CartId == User.Identity.Name);
            decimal OrderTotal = (from c in CartItems
                                  select (int)c.Count * c.Album.Price).Sum();
            order.Total = OrderTotal;

            // save main order
            db.Orders.Add(order);
            db.SaveChanges();

            // now save each item
            foreach (Cart item in CartItems)
            {
                OrderDetail od = new OrderDetail
                {
                    OrderId = order.OrderId,
                    AlbumId = item.AlbumId,
                    Quantity = item.Count,
                    UnitPrice = item.Album.Price
                };

                db.OrderDetails.Add(od);
            }

            // save all the order items
            db.SaveChanges();

            // show the confirmation / receipt / invoice page 
            return RedirectToAction("Details", "Orders", new { id = order.OrderId });
        }

        private void MigrateCart()
        {
            // attach anonymous cart to an authenticated once they log in
            if (!String.IsNullOrEmpty(Session["CartId"].ToString()) && User.Identity.IsAuthenticated)
            {
                if (Session["CartId"].ToString() != User.Identity.Name)
                {
                    // get cart items with the random id
                    string CurrentCartId = Session["CartId"].ToString();
                    var CartItems = db.Carts.Where(c => c.CartId == CurrentCartId);

                    foreach (Cart item in CartItems)
                    {
                        item.CartId = User.Identity.Name;
                    }

                    db.SaveChanges();

                    // update the session variable to the username
                    Session["CartId"] = User.Identity.Name;
                }
            }
        }
    }
}