using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Hackathon.BeesPortal.Web.Models;
using Hackathon.BeesPortal.Web.Models.Portal;

namespace Hackathon.BeesPortal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Test(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.Viewed = true;

                _context.SaveChanges();
            }

            var apiaries = new List<Apiary>();
            var hives = new List<Hive>();
            var notifications = new List<Notification>();
            var dataSegments = new List<DataSegment>();

            apiaries.AddRange(_context.Apiaries.Where(a => a.Username == User.Identity.Name).ToList());

            foreach (var apiary in apiaries)
            {
                hives.AddRange(_context.Hives.Where(h => h.ApiaryId == apiary.Id).ToList());
            }

            foreach (var hive in hives)
            {
                notifications.AddRange(_context.Notifications.Where(n => n.SigfoxId == hive.SigfoxId && n.Viewed == false).ToList());
            }

            foreach (var hive in hives)
            {
                dataSegments.AddRange(_context.DataSegments.Where(n => n.SigfoxId == hive.SigfoxId).ToList());
            }

            var viewModel = new DataSegmentViewModel();
            viewModel.Apiaries = apiaries;
            viewModel.Hives = hives;
            viewModel.Notifications = notifications;
            viewModel.DataSegments = dataSegments;

            return View("Index", viewModel);
        }

        public ActionResult Index()
        {
            //var random = new Random();

            //float[] temperatures =
            //{
            //    (float) 14.6,
            //    (float) 16.9,
            //    (float) 19.2,
            //    (float) 23.5,
            //    (float) 26.7,
            //    (float) 29.3,
            //    (float) 32.4,
            //    (float) 35.1,
            //    (float) 38.8,
            //    (float) 40.6
            //};

            //float[] humidity =
            //{
            //    (float) 82.6,
            //    (float) 84.9,
            //    (float) 86.3,
            //    (float) 88.7,
            //    (float) 90.5,
            //    (float) 92.4,
            //    (float) 94.3,
            //    (float) 96.8,
            //    (float) 98.4,
            //    (float) 99.9
            //};

            //var nowminusmonth = DateTime.Now.AddDays(-30);

            //for (var i = 0; i < 750; i++)
            //{
            //    nowminusmonth = nowminusmonth.AddHours(1);

            //    var segment = new DataSegment
            //    {
            //        SigfoxId = "74B30",
            //        Temperature = temperatures[random.Next(0, 10)],
            //        Humidity = humidity[random.Next(0, 10)],
            //        DateTime = nowminusmonth
            //    };

            //    _context.DataSegments.Add(segment);
            //    _context.SaveChanges();
            //}

            var apiaries = new List<Apiary>();
            var hives = new List<Hive>();
            var notifications = new List<Notification>();
            var dataSegments = new List<DataSegment>();

            apiaries.AddRange(_context.Apiaries.Where(a => a.Username == User.Identity.Name).ToList());

            foreach (var apiary in apiaries)
            {
                hives.AddRange(_context.Hives.Where(h => h.ApiaryId == apiary.Id).ToList());
            }

            foreach (var hive in hives)
            {
                notifications.AddRange(
                    _context.Notifications.Where(n => n.SigfoxId == hive.SigfoxId && n.Viewed == false).ToList());
            }

            foreach (var hive in hives)
            {
                dataSegments.AddRange(_context.DataSegments.Where(n => n.SigfoxId == hive.SigfoxId).ToList());
            }

            var viewModel = new DataSegmentViewModel();
            viewModel.Apiaries = apiaries;
            viewModel.Hives = hives;
            viewModel.Notifications = notifications;
            viewModel.DataSegments = dataSegments;

            return View(viewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}