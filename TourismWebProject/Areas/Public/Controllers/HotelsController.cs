﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using TourismWebProject.Models;
using System.IO;
using System.Data.Entity.SqlServer;

namespace TourismWebProject.Areas.Public.Controllers
{
    public class HotelsController : Controller
    {
        TourismDbContext db = new TourismDbContext();

        // GET: Public/Hotels
        public ActionResult Index()
        {
            //filer results see action "Search"
            ViewData["Country"] = TempData["CountryName"];
            ViewData["City"] = TempData["CityName"];
            ViewData["Rating"] = TempData["Rating"];
            ViewData["AvailableHotels"] = TempData["AvailableHotels"];
            //-----
            HotelViewModel hotelViewModel = new HotelViewModel()
            {
                Hotel = db.Hotel.Include(x => x.HotelRoom).Include(x => x.Rating).Where(i => i.HotelStatus == 1).ToList(),
                HotelPage = db.HotelPage.ToList()
            };
            ViewData["PageNum"] = TempData["PageNum"];
            ViewBag.StarRating = db.Rating.ToList();

            return View(hotelViewModel);
        }

        //Single Hotel
        public ActionResult Single(int? id)
        {

            var dir = Server.MapPath("~\\HotelItems");

            foreach (var item in db.Hotel.ToList())
            {
                if (item.HotelId == id)
                {
                    ViewData["HotelId"] = id;
                    ViewData["HotelName"] = item.HotelName;
                    ViewData["HotelAddress"] = item.HotelAddress;
                    ViewData["RatingId"] = item.RatingId;
                    ViewData["HotelAddress"] = item.HotelAddress;
                    ViewData["HotelDescription"] = item.HotelDescription;

                    ViewData["HotelCountry"] = item.HotelCountry;
                    ViewData["HotelCity"] = item.HotelCity;
                    ViewData["HotelRating"] = item.RatingId;

                    var file = Path.Combine(dir, item.HotelPic);
                    var fileContent = System.IO.File.ReadAllText(file);
                    ViewData["HotelPics"] = fileContent;

                }
            }

            HotelViewModel blogViewModel = new HotelViewModel()
            {
                Hotel = db.Hotel.Include(x => x.HotelRoom).Include(x => x.Rating).Where(f => f.HotelId != id).ToList(),
                HotelRoom = db.HotelRoom.Include(z => z.room).Include(e => e.hotel).Where(i => i.Status == 1).ToList(),
                HotelPage = db.HotelPage.ToList()
            };

            List<Room> room = new List<Room>();
            ViewData["BookStatus"] = TempData["BookStatus"];
            foreach (var item in db.HotelRoom.Where(i => i.HotelId == id))
            {
                room.Add(item.room);
            }
            ViewBag.Rooms = room;
            ViewBag.StarRating = db.Rating.ToList();

            return View(blogViewModel);
        }

        //get items in a particular page
        public ActionResult Page(int? id)
        {
            TempData["PageNum"] = id - 1;
            return RedirectToAction("Index");
        }

        //Booking proccess
        public ActionResult Booking(Reservation reservation, Room room, Hotel hotel)
        {

            TempData["BookStatus"] = 2;
            //possibility to book only during the year// refresh page if the below statement is false
            if (reservation.ReservationDateFrom.Year == DateTime.Today.Year && reservation.ReservationDateTo.Year == DateTime.Today.Year && reservation.ReservationDateFrom.Month >= DateTime.Today.Month && reservation.ReservationDateTo.Month >= DateTime.Today.Month)
            {
                if (reservation.ReservationDateFrom < reservation.ReservationDateTo)
                {

                    //get the number of booked hotel  in the reservation list
                    var countofReservation = db.Reservation.Where(i => (i.ReservationServiceTypeId == hotel.HotelId) && (i.ReservationServiceId == room.RoomId) && (i.ReservationDateFrom.Month == reservation.ReservationDateFrom.Month) && (i.ReservationDateTo.Month == reservation.ReservationDateTo.Month)).Count();

                    //uses to check avilability of dates (counts rooms)
                    var CheckCount = 0;

                    var dateFrom = reservation.ReservationDateFrom;
                    var dateTo = reservation.ReservationDateTo;

                    //calculate max reservation period (max 30 days)
                    int Month = dateTo.Month - dateFrom.Month;

                    int Days = dateTo.Day - dateFrom.Day;

                    int Reservationlimit = 0;

                    var count = 0;

                    if (Month == 1)
                    {
                        Reservationlimit = 31 - dateFrom.Day + dateTo.Day;
                    }
                    //-----

                    if (reservation.ReservationDateFrom != null && reservation.ReservationDateTo != null)
                    {
                        //check max stay period (30 days)
                        if (Month == 0 || Month == 1 && Reservationlimit < 32)
                        {
                            foreach (var item in db.Reservation.Where(i => (i.ReservationServiceTypeId == hotel.HotelId) && (i.ReservationServiceId == room.RoomId) && (i.ReservationDateFrom.Month == reservation.ReservationDateFrom.Month) && (i.ReservationDateTo.Month == reservation.ReservationDateTo.Month)).ToList())
                            {
                                count++;
                                //--
                                if (dateFrom.Day < item.ReservationDateFrom.Day && item.ReservationDateFrom.Day > dateTo.Day || dateFrom.Day > item.ReservationDateTo.Day && item.ReservationDateTo.Day < dateTo.Day)
                                {
                                    CheckCount++;

                                    if (countofReservation == CheckCount)
                                    {
                                        if (Month == 1)
                                        {
                                            TempData["Total"] = 31 - dateFrom.Day + dateTo.Day;
                                        }
                                        else
                                        {
                                            TempData["Total"] = Days;
                                        };
                                        TempData["HotelId"] = hotel.HotelId;
                                        TempData["RoomId"] = room.RoomId;
                                        TempData["DataFrom"] = reservation.ReservationDateFrom;
                                        TempData["DataTo"] = reservation.ReservationDateTo;
                                     
                                        return RedirectToAction("ConfirmBooking");
                                    }
                                }
                            }

                            //if there is no such hotel in the reservation list
                            if (count == 0)
                            {
                                //Reservation(reservation, hotel, room);
                                if (Month == 1)
                                {
                                    TempData["Total"] = 31 - dateFrom.Day + dateTo.Day;
                                }
                                else
                                {
                                    TempData["Total"] = Days;
                                };
                                TempData["HotelId"] = hotel.HotelId;
                                TempData["RoomId"] = room.RoomId;
                                TempData["DataFrom"] = reservation.ReservationDateFrom;
                                TempData["DataTo"] = reservation.ReservationDateTo;

                                return RedirectToAction("ConfirmBooking");
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Single/" + hotel.HotelId.ToString());
        }

        //Confirm payment
        public ActionResult ConfirmBooking()
        {

            return View();
        }

        //after successful payment
        public ActionResult Successful()
        {
            TempData["BookStatus"] = 1;
            Room room = new Room();
            Reservation reservation = new Reservation();

            reservation.ReservationServiceTypeId = Convert.ToInt32(TempData["HotelId"]);
            reservation.ReservationServiceId = Convert.ToInt32(TempData["RoomId"]);
            reservation.ReservationDateFrom = Convert.ToDateTime(TempData["DataFrom"]);
            reservation.ReservationDateTo = Convert.ToDateTime(TempData["DataTo"]);
            Reservation(reservation);

            return RedirectToAction("Single/" + TempData["HotelId"].ToString());
        }

        //filter for searching hotels-------------------------------------------------
        //Possible search by:
        //1)Country,City
        //2)Country
        //3)Country,City,Rating
        //4)Country,Rating
        //5)Country ,city ,dates
        //6)Rating (website is oriented to at least "3 star rating" hotels, choosing star rating 1 or 2  will show all hotels)
        public ActionResult Search([Bind(Exclude = "ReservationDateFrom,ReservationDateTo,RoomPrice,ReservationDateFrom,ReservationDateTo")] Hotel hotel, [Bind(Include = "ReservationDateFrom,ReservationDateTo")]Reservation reservation)
        {
            //set all hotels status 1(visible)
            HotelStatus();

            //set all romms status1(available)
            RoomStatus();

            //Country ,city
            if (hotel.HotelCity != null && hotel.HotelCountry != null && Convert.ToInt32(hotel.RatingId) == 1)
            {
                var count = 0;
                foreach (var item in db.Hotel.ToList())
                {
                    if (item.HotelCountry == hotel.HotelCountry && item.HotelCity == hotel.HotelCity)
                    {
                        count++;
                        TempData["CountryName"] = item.HotelCountry;
                        TempData["CityName"] = item.HotelCity;
                        break;
                    }
                }
                if (count == 0)
                {
                    TempData["CountryName"] = "No such country";
                    TempData["CityName"] = "No such city";
                }
            }
            else
            {
                //if only country selected
                if (hotel.HotelCity == null && Convert.ToInt32(hotel.RatingId) == 1)
                {
                    var count = 0;
                    foreach (var item in db.Hotel.ToList())
                    {
                        if (hotel.HotelCountry == item.HotelCountry)
                        {
                            count++;
                            TempData["CountryName"] = item.HotelCountry;
                            break;
                        }
                    }

                    if (count == 0)
                    {
                        TempData["CountryName"] = "No such country";
                    }
                }
            }


            //if selected:Country,City,Rating
            if (hotel.HotelCity != null && hotel.HotelCountry != null && Convert.ToInt32(hotel.RatingId) != 1)
            {
                var count = 0;
                foreach (var item in db.Hotel.ToList())
                {
                    if (item.HotelCountry == hotel.HotelCountry && item.HotelCity == hotel.HotelCity && item.RatingId == hotel.RatingId)
                    {
                        count++;
                        TempData["CountryName"] = item.HotelCountry;
                        TempData["CityName"] = item.HotelCity;
                        TempData["Rating"] = item.RatingId;
                        break;
                    }
                }
                if (count == 0)
                {
                    TempData["CountryName"] = "No such country";
                }
            }
            //if selected:Country,Rating
            if (hotel.HotelCity == null && hotel.HotelCountry != null && Convert.ToInt32(hotel.RatingId) != 1)
            {
                foreach (var item in db.Hotel.ToList())
                {
                    if (item.HotelCountry == hotel.HotelCountry && item.RatingId == hotel.RatingId)
                    {
                        TempData["CountryName"] = item.HotelCountry;
                        TempData["Rating"] = item.RatingId;
                        break;
                    }
                }
            }
            //if selected:Rating
            if (hotel.HotelCity == null && hotel.HotelCountry == null && Convert.ToInt32(hotel.RatingId) != 1 || hotel.HotelCity == null && hotel.HotelCountry == null && Convert.ToInt32(hotel.RatingId) != 2)
            {
                foreach (var item in db.Hotel.ToList())
                {
                    if (item.RatingId == hotel.RatingId)
                    {
                        TempData["Rating"] = item.RatingId;
                        break;
                    }
                }
            }


            //if selected:Country ,city ,dates
            if (hotel.HotelCity != null && hotel.HotelCountry != null && Convert.ToInt32(hotel.RatingId) == 1 && reservation.ReservationDateFrom.Year != 0001 && reservation.ReservationDateTo.Year != 0001)
            {
                List<Hotel> AvailableHotels = new List<Hotel>();

                foreach (var item in db.Hotel.ToList())
                {
                    if (item.HotelCountry == hotel.HotelCountry && item.HotelCity == hotel.HotelCity)
                    {
                        if (db.Reservation.Any(w => w.ReservationServiceTypeId == item.HotelId))
                        {
                            foreach (var item1 in db.Reservation.Where(d => d.ReservationServiceTypeId == item.HotelId).ToList())
                            {
                                foreach (var item2 in db.HotelRoom.ToList())
                                {

                                    if (item2.HotelId == item1.ReservationServiceTypeId && item2.RoomId == item1.ReservationServiceId)
                                    {

                                        if (reservation.ReservationDateFrom.Month != item1.ReservationDateFrom.Month && item1.ReservationDateTo.Month != reservation.ReservationDateTo.Month || reservation.ReservationDateFrom.Month == item1.ReservationDateFrom.Month && item1.ReservationDateTo.Month == reservation.ReservationDateTo.Month && reservation.ReservationDateFrom.Day < item1.ReservationDateFrom.Day && item1.ReservationDateFrom.Day > reservation.ReservationDateTo.Day || reservation.ReservationDateFrom.Day > item1.ReservationDateTo.Day && item1.ReservationDateTo.Day < reservation.ReservationDateTo.Day && reservation.ReservationDateFrom.Month == item1.ReservationDateFrom.Month && item1.ReservationDateTo.Month == reservation.ReservationDateTo.Month)
                                        {

                                        }
                                        //case
                                        else
                                        {

                                            foreach (var item4 in db.HotelRoom.Where(s => (s.HotelId == item1.ReservationServiceTypeId) && (s.RoomId == item1.ReservationServiceId)).ToList())
                                            {//don't show  a room
                                                item4.Status = 2;
                                                db.SaveChanges();
                                            }
                                            CheckHotelAvailability(item);

                                        }

                                    }
                                }
                            }
                        }
                    }

                    AvailableHotels.Add(item);
                    TempData["AvailableHotels"] = AvailableHotels;
                }
            }
            return RedirectToAction("Index");
        }

        //check availability of rooms of a hotel
        public void CheckHotelAvailability(Hotel hotel)
        {
            int RoomsCount = HotelRooms(hotel);

            int count = 0;
            foreach (var item in db.HotelRoom.Where(i => (i.HotelId == hotel.HotelId) && (i.Status == 2)))
            {
                count++;
            }

            if (count == RoomsCount)
            {
                hotel.HotelStatus = 2;
                db.SaveChanges();
            }
        }

        //Check Amount of a hotel's rooms
        public int HotelRooms(Hotel hotel)
        {
            int count = 0;
            foreach (var item in db.HotelRoom.Where(i => i.HotelId == hotel.HotelId))
            {
                count++;
            }

            return count;
        }

        //set all hotels status 1(visible)
        public void HotelStatus()
        {
            foreach (var item in db.Hotel.ToList())
            {
                item.HotelStatus = 1;
                db.SaveChanges();
            }
        }


        //set all rooms available
        public void RoomStatus()
        {
            foreach (var item in db.HotelRoom.ToList())
            {
                item.Status = 1;
                db.SaveChanges();
            }
        }
        //---------------------------------------------------------------------

        //make reservation
        public void Reservation(Reservation reservation)
        {
            reservation.UserId = 1;//========================================change it!===========================
            reservation.ReservationStatus = true;
            reservation.ServiceTypeId = 1;
            foreach (var item1 in db.Room.Where(i => i.RoomId == reservation.ReservationServiceId))
            {
                reservation.ReservationTotal = item1.RoomPrice * Convert.ToInt32(TempData["Total"]);
            }
            db.Reservation.Add(reservation);
            db.SaveChanges();
        }
    }
}
