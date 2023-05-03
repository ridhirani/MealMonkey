﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MealMonkey.Models;
using MealMonkey.ViewModel;
using Microsoft.AspNet.Identity;

namespace MealMonkey.Controllers
{
    public class ProductsController : Controller
    {
        private masterEntities mdb = new masterEntities();
        private ProductEntities db = new ProductEntities();


        // GET: Products
        public ActionResult Index(int? id)
        {

            var products = db.MM_Products.ToList();
            var categories = mdb.MM_Categories.ToList();
            ProductViewModel productViewModel = new ProductViewModel();

            if (id == null || id == 0)
            {
                
                productViewModel.Categories = categories;
                productViewModel.Products = products;
                return View(productViewModel);
            }
            
            var res = from p in products where p.CategoryId == id select p;

            productViewModel.Categories = categories;
            productViewModel.Products = res;
            
            return View(productViewModel);
        }

        [Authorize]
        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            IsLoggedIn();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MM_Products mM_Products = db.MM_Products.Find(id);
            if (mM_Products == null)
            {
                return HttpNotFound();
            }
            TempData["ProductId"] = id;

            return View(mM_Products);
        }

        [Authorize]
        public ActionResult CartTable()
        {
            IsLoggedIn();
            int k = Convert.ToInt32(Session["UserId"]);
            dynamic dy = new ExpandoObject();
            dy.Carts = getCarts(k);
            dy.Products = getProducts();

            return View(dy); 
        }
        [Authorize]
        //Helping Methods
        public IEnumerable<MM_Carts> getCarts(int? K)
        {
            IsLoggedIn();
            //int K = Convert.ToInt32(Session["UserId"]);
            IEnumerable<MM_Carts> Carts = mdb.MM_Carts.ToList();
            var FilteredResult = from s in Carts where s.UserId == K select s;

            return FilteredResult;

        }
        [Authorize]
        public List<MM_Products> getProducts()
        {
            List<MM_Products> Products = db.MM_Products.ToList();

            return Products;

        }
        //Edit Converted to cart
        // GET: Products/Cart/5

        [Authorize]
        //Create cart
        public ActionResult Cart(int? id)
        {
            IsLoggedIn();
            return View();
        }

        // POST: Products/Cart/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Cart([Bind(Include ="Cartid,Productid,Quantity,UserId")] MM_Carts mM_Carts)
        {
            IsLoggedIn();
            MM_Carts temp = new MM_Carts();


            if (ModelState.IsValid)
            {
                temp.ProductId = Convert.ToInt32( TempData["ProductId"]);
                temp.UserId = Convert.ToInt32(Session["UserId"]);
                temp.CartId = mM_Carts.CartId;
                temp.Quantity = mM_Carts.Quantity;
                

                mdb.MM_Carts.Add(temp);
                
                
                mdb.SaveChanges();
                return RedirectToAction("CartTable");
            }

            return View(mM_Carts);
        }
        [Authorize]
        // GET: Products/CartDelete/5
        public ActionResult CartDelete(int? id)
        {
            IsLoggedIn();
            MM_Carts mM_Cart = mdb.MM_Carts.Find(id);
            mdb.MM_Carts.Remove(mM_Cart);
            mdb.SaveChanges();
            return RedirectToAction("CartTable");
            
        }
        [Authorize]
        public ActionResult CartToOrder()
        {
            IsLoggedIn();
            int K = Convert.ToInt32(Session["UserId"]);
            IEnumerable<MM_Carts> Carts = mdb.MM_Carts.ToList();
            var FilteredResult = from s in Carts where s.UserId == K select s;
            String Oid = Guid.NewGuid().ToString();

            foreach (var items in FilteredResult)
            {
                MM_Orders temp = new MM_Orders();
                //var k = ViewBag.TotalAmount;

                temp.ProductId = items.ProductId;
                temp.Quantity = items.Quantity;
                temp.OrderDate = DateTime.Now;
                temp.UserId = items.UserId;
                temp.Status = "Order Created";
                temp.PaymentId = Convert.ToInt32(Session["Amount"]);//Doubtful
                temp.OrderNo =  Oid;
                //temp.Amount = k;



                if (ModelState.IsValid)
                {

                    mdb.MM_Orders.Add(temp);
                    mdb.SaveChanges();

                    while (mdb.MM_Carts.Where(x => x.UserId== K).Count() != 0)
                    {
                        var t = mdb.MM_Carts.Where(x => x.UserId == K).FirstOrDefault();
                        mdb.MM_Carts.Remove(t);
                        mdb.SaveChanges();
                    }

                }
            }
            return RedirectToAction("OrderList");

            //return View();

        }
        [Authorize]
        public ActionResult OrderList()
        {
            IsLoggedIn();
            int k = Convert.ToInt32(Session["UserId"]);
            IEnumerable<MM_Orders> Orders = mdb.MM_Orders.ToList();
            IEnumerable<MM_Products> Product = db.MM_Products.ToList();

            IEnumerable <OrderRes> OrderList = Orders
                .Where(u => u.UserId == k)
                .GroupBy(o => new { o.OrderNo, o.UserId,o.PaymentId,o.Status,o.OrderDate})
                .Select(p => new OrderRes
                {
                    OrderNo = p.Key.OrderNo,
                    Quantity = (int)p.Sum(t => t.Quantity),
                    Count = p.Count(),
                    PaymentId = (int)p.Key.PaymentId,
                    Status = (string)p.Key.Status,
                    Date = (DateTime)p.Key.OrderDate
                    

                }).OrderByDescending(x => x.Date).ToList();
            
            //var z = OrderList;
            return View(OrderList);

        }
        public ActionResult OrderDetails(string id)
        {
            int k = Convert.ToInt32(Session["UserId"]);
            IEnumerable<MM_Orders> mM_Orders = mdb.MM_Orders.ToList();
            IEnumerable<MM_Products> mM_Products = db.MM_Products.ToList();

            IEnumerable<OrderDetailsViewModel> od = mM_Orders
                    .Where(u => u.UserId == k && u.OrderNo == id)
                    .Select(p => new OrderDetailsViewModel
                    {

                        ProductName = (string) mM_Products.Where(w => w.ProductId == p.ProductId)
                                            .Select(l => l.Name).FirstOrDefault(),
                        Quantity = (int)p.Quantity,
                        Price = (decimal)mM_Products
                                            .Where(w => w.ProductId == p.ProductId)
                                            .Select(l => l.Price).FirstOrDefault()


                    }).ToList();

            return View(od);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private void IsLoggedIn()
        {
            //if(Session["UserId"] == null)
            //{
            //    FormsAuthentication.SignOut();
            //}
        }
    }
}
