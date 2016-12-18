using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using PagedList;
using PagedList.Mvc;
using System.Xml;
using System.ServiceModel.Syndication;

namespace WebApplication1.Controllers
{
    public class TinTucController : Controller
    {
        dbTinTucDataContext data = new dbTinTucDataContext();
        // GET: TinTuc
        public ActionResult Index()
        {
            var tinmoi = Laytinmoi(10);
            return View(tinmoi);
        }
        public ActionResult SlideTinMoi()
        {
            var tinmoi = Laytinmoi(5);
            return PartialView(tinmoi);
        }
        public ActionResult TopTinGame()
        {
            var ttg = (from tg in data.BangTins where tg.LoaiTinID == 1 select tg).OrderByDescending(a => a.NgayDang).Take(1).ToList();
            return PartialView(ttg.Single());
        }
        public ActionResult TopTinEsport()
        {
            var tes = (from ep in data.BangTins where ep.LoaiTinID == 2 select ep).OrderByDescending(a => a.BangTinID).Take(1).ToList();
            return PartialView(tes.Single());
        }
        public ActionResult TopCamNang()
        {
            var tcn = (from cn in data.BangTins where cn.LoaiTinID == 3 select cn).OrderByDescending(a => a.BangTinID).Take(1).ToList();
            return PartialView(tcn.Single());
        }
        public ActionResult TopCongDong()
        {
            var tcd = (from cd in data.BangTins where cd.LoaiTinID == 4 select cd).OrderByDescending(a => a.BangTinID).Take(1).ToList();
            return PartialView(tcd.Single());
        }

        private List<BangTin> Laytinmoi (int count)
        {
            return data.BangTins.OrderByDescending(a => a.NgayDang).Take(count).ToList();
        }
        public ActionResult Theloai()
        {
            var theloai = from tl in data.LoaiTins select tl;
            return PartialView(theloai);
        }
        public ActionResult Tintheotheloai(int id, int ? page)
        {
            int pageSize = 20;
            int pageNum = (page ?? 1);
            var tin = (from s in data.BangTins where s.LoaiTinID == id select s).OrderByDescending(a => a.BangTinID);
            return View(tin.ToPagedList(pageNum, pageSize));
        }
        public ActionResult Chitiettin(int id)
        {
            var ngay = from s in data.BangTins where s.BangTinID == id select s.NgayDang.ToLongDateString();
            ViewBag.NgayDang = ngay.First();
            var tin = from s in data.BangTins
                      where s.BangTinID == id
                      select s;
            return View(tin.Single());
        }
        public ActionResult TinEsport()
        {
            var tin = (from t in data.BangTins 
                       where t.LoaiTinID == 2 
                       select t).OrderByDescending(a => a.BangTinID).Skip(1).Take(4).ToList();
            return PartialView(tin);
        }
        public ActionResult TinGame()
        {
            var tin = (from t in data.BangTins
                       where t.LoaiTinID == 1
                       select t).OrderByDescending(a => a.BangTinID).Skip(1).Take(4).ToList();
            return PartialView(tin);
        }
        public ActionResult TinCamNang()
        {
            var tin = (from t in data.BangTins
                       where t.LoaiTinID == 3
                       select t).OrderByDescending(a => a.BangTinID).Skip(1).Take(4).ToList();
            return PartialView(tin);
        }
        public ActionResult TinCongDong()
        {
            var tin = (from t in data.BangTins
                       where t.LoaiTinID == 4
                       select t).OrderByDescending(a => a.BangTinID).Skip(1).Take(4).ToList();
            return PartialView(tin);
        }
        public ActionResult BangQuangCao()
        {
            var qc = (from qcao in data.QuangCaos
                      where qcao.ViTri == 2
                      select qcao).OrderByDescending(a => a.NgayDangLen).Take(2).ToList();
            return PartialView(qc);
        }
        public ActionResult Banner()
        {
            var qc = (from qcao in data.QuangCaos
                      where qcao.ViTri == 1
                      select qcao).OrderBy(a => a.QuangCaoID).Take(1).ToList();
            return PartialView(qc.Single());
        }
        public ActionResult Bannerleft()
        {
            var qc = (from qcao in data.QuangCaos
                      where qcao.ViTri == 3
                      select qcao).OrderBy(a => a.QuangCaoID).Take(1).ToList();
            return PartialView(qc.Single());
        }
        public ActionResult Bannerright()
        {
            var qc = (from qcao in data.QuangCaos
                      where qcao.ViTri == 4
                      select qcao).OrderBy(a => a.QuangCaoID).Take(1).ToList();
            return PartialView(qc.Single());
        }

        public ActionResult HinhAnh()
        {
            var hinh = (from ha in data.HinhAnhs
                        select ha).OrderByDescending(a => a.HinhAnhID).Take(6).ToList();
            return PartialView(hinh);
        }
        public ActionResult Category()
        {
            var theloai = from tl in data.LoaiTins select tl;
            return PartialView(theloai);
        }
        public ActionResult TinNoiBat()
        {
            var tinmoi = Laytinmoi(15);
            return PartialView(tinmoi);
        }
        public ActionResult TinLienQuan(int id)
        {
            var theloai = (from e in data.BangTins
                          where e.LoaiTinID == (from tin in data.BangTins where tin.BangTinID == id select tin.LoaiTinID).First() 
                          && e.BangTinID != (from s in data.BangTins where s.BangTinID == id select s.BangTinID).First()
                          select e).OrderByDescending(a=>a.NgayDang).Take(4).ToList();
            return PartialView(theloai);
        }

      }
}