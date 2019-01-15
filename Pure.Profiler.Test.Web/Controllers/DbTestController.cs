//using Microsoft.AspNetCore.Mvc;
//using Pure.Profiler.DbProfilingStorage;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Pure.Profiler.Test.Web.Controllers
//{
//    public class DbTestController : Controller
//    {
     

//        public IActionResult Echo()
//        {
//             ViewBag.Msg = "Hello " + DateTime.Now;

//            return View();
//        }


//        public IActionResult Test()
//        {
//            var v = new TestEntity();
//            using (var db = new PureProfilingDbContext())
//            {
//                //v.SEQ = Guid.NewGuid().ToString();
//                v.Id = Guid.NewGuid().ToString();
//                v.Started = DateTime.Now;
//                v.MachineName = "test";
//                db.Insert<TestEntity>(v, null);

//                ViewBag.Data = v;


//                v.MachineName = "test" + DateTime.Now;
//                db.Update<TestEntity>(v, null);
//            }

         
             

//            return View();
//        }

//        public IActionResult Test2()
//        {
            
//            using (var db = new PureProfilingDbContext())
//            {
//                db.ExecuteList<TestEntity>("select ggg from sys_testdata ", null).ToList();

//                int total = 0;
//                ViewBag.Data2 = db.GetPage<TestEntity>(1, 5 ,null , null , out total, null, null, true).ToList();
//            }



//            return View();
//        }
//    }
//}
