using Microsoft.AspNetCore.Mvc;
using Pure.Profiler.DbProfilingStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pure.Profiler.Test.Web.Controllers
{
    public class DbTestController : Controller
    {
     

        public IActionResult Echo()
        {
             ViewBag.Msg = "Hello " + DateTime.Now;

            return View();
        }


        public IActionResult Test()
        {
            var v = new TestEntity();
            using (var db = new PureProfilingDbContext())
            {
                v.SEQ = Guid.NewGuid().ToString();
                v.Id = Guid.NewGuid().ToString();
                v.Started = DateTime.Now;
                v.MachineName = "test";
                db.Insert<TestEntity>(v, null);
            }

            ViewBag.Data = v;

            return View();
        }
    }
}
