using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using PagedList;
using PagedList.Mvc;
using System.IO;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        dbTinTucDataContext db = new dbTinTucDataContext();
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Logup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logup(FormCollection collection, Admin ad)
        {
            var hoten = collection["form-hoten"];
            var email = collection["form-email"];
            var sdt = collection["form-sdt"];
            var gioitinh = collection["form-gioitinh"];
            var tendn = collection["form-ID"];
            var pass = collection["form-password"];
            var repass = collection["form-repassword"];
            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Vui lòng điền đầy đủ họ tên của bạn";
            }
            else if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi2"] = "Vui lòng nhập địa chỉ mail của bạn";
            }
            else if (String.IsNullOrEmpty(sdt))
            {
                ViewData["Loi3"] = "Vui lòng nhập số điện thoại liên lạc của bạn";
            }
            else if (String.IsNullOrEmpty(gioitinh))
            {
                ViewData["Loi4"] = "Vui lòng nhập giới tính của bạn";
            }
            else if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi5"] = "Vui lòng nhập tên để bạn đăng nhập";
            }
            else if (String.IsNullOrEmpty(pass))
            {
                ViewData["Loi6"] = "Vui lòng nhập mật khẩu";
            }
            else if (String.IsNullOrEmpty(repass))
            {
                ViewData["Loi8"] = "Vui lòng nhập lại mật khẩu vừa nhập";
            }
            else if (pass != repass)
            {
                ViewData["Loi7"] = "Mật khẩu nhập lai không trùng với mật khẩu vừa khai báo";
            }
            else
            {
                ad.HoTen = hoten;
                ad.Email = email;
                ad.SDT = int.Parse(sdt);
                ad.GioiTinh = gioitinh;
                ad.AdminID = tendn;
                ad.Password = pass;
                db.Admins.InsertOnSubmit(ad);
                db.SubmitChanges();
                return RedirectToAction("Login");
            }
            return this.Logup();
        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["form-username"];
            var matkhau = collection["form-password"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Nhập tên đăng nhập nhé";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Nhập mật khẩu nhé";
            } 
            else
            {
                Admin ad = db.Admins.SingleOrDefault(n => n.AdminID == tendn && n.Password == matkhau);
                if(ad != null)
                {
                    Session["Username"] = collection["form-username"];
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Login","Admin");
        }

        public ActionResult Index(int? page)
        {

            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login","Admin");
            }
            else
            {
                int pageNum = (page ?? 1);
                int pageSize = 7;
                ViewBag.UN = Session["Username"];
                var ad = (from tin in db.BangTins where tin.MaAdmin == (from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin).First() 
                          select tin).OrderByDescending(a => a.NgayDang).ToList();
                return View(ad.ToPagedList(pageNum, pageSize));
            }
        }

        public ActionResult TinTuc(int ? page, BangTin tin)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login","Admin");
            }
            else
            {
                ViewBag.UN = Session["Username"];
                var ad = (from tt in db.BangTins
                          where tt.MaAdmin == (from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin).First()
                          select tt).OrderByDescending(a => a.NgayDang).ToList();
                return View(ad.ToPagedList(pageNum, pageSize));
            }
        }

        public ActionResult Create()
        {
            ViewBag.LoaiTinID = new SelectList(db.LoaiTins.ToList().OrderBy(n => n.TenLoaiTin), "LoaiTinID", "TenLoaiTin");
            ViewBag.AdminID = new SelectList(db.Admins.ToList().OrderBy(n => n.AdminID), "MaAdmin", "AdminID");
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View();
        }

        [HttpPost]
        [ValidateInput(false)]        
        public ActionResult Create(BangTin tin, HttpPostedFileBase fileupload, FormCollection collection)
        {
            var ad = from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin;
            var td = collection["TieuDe"];
            var mt = collection["MoTa"];
            var nd = collection["Noidung"];
            var ng = collection["NgayDang"];
            ViewBag.LoaiTinID = new SelectList(db.LoaiTins.ToList().OrderBy(a => a.TenLoaiTin), "LoaiTinID", "TenLoaiTin");
            if (fileupload == null)
            {
                ViewData["Loi4"] = "Vui lòng chọn hình ảnh cho bảng tin";
            }
            else if (String.IsNullOrEmpty(td))
            {
                ViewData["Loi1"] = "Tiêu đề của bảng tin là gì vậy ?";
            }
            else if (String.IsNullOrEmpty(mt))
            {
                ViewData["Loi2"] = "Viết vài dòng mô tả cho bảng tin đi";
            }
            else if (String.IsNullOrEmpty(nd))
            {
                ViewData["Loi3"] = "Nội dung của bảng tin là gì ?";
            }
            else if (String.IsNullOrEmpty(ng))
            {
                ViewData["Loi5"] = "Ngày đăng của bảng tin này là hôm nay đó";
            }
            else
            {
                if (ModelState.IsValid)
                { 
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhTin"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Message = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    tin.MaAdmin = ad.Single();
                    tin.AnhBiaTin = fileName;
                    db.BangTins.InsertOnSubmit(tin);
                    db.SubmitChanges();
                    return RedirectToAction("TinTuc");
                }
            }
            return this.Create();
        }

        public ActionResult Details(int id)
        {
            BangTin tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
            ViewBag.BangTinID = tin.BangTinID;
            if(tin==null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(tin);
        }

        public ActionResult Edit(int id)
        {
            BangTin tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
            ViewBag.LoaiTinID = new SelectList(db.LoaiTins.ToList().OrderBy(n => n.TenLoaiTin), "LoaiTinID", "TenLoaiTin", tin.LoaiTinID);
            ViewBag.AdminID = new SelectList(db.Admins.ToList().OrderBy(n => n.MaAdmin), "AdminID", "AdminID", tin.MaAdmin);
            if(tin==null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(tin);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateInput(false)]
        public ActionResult ConfirmOnEdit(BangTin tin, int id, HttpPostedFileBase fileupload, FormCollection collection)
        {
            var tieude = collection["TieuDe"];
            var ngaydang = collection["NgayDang"];
            var mota = collection["MoTa"];
            var noidung = collection["NoiDung"];
            var anhbia = collection["AnhBiaTin"];
            tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
            ViewBag.LoaiTinID = new SelectList(db.LoaiTins.ToList().OrderBy(n => n.TenLoaiTin), "LoaiTinID", "TenLoaiTin");
            ViewBag.AdminID = new SelectList(db.Admins.ToList().OrderBy(n => n.MaAdmin), "MaAdmin", "AdminID");
            if (String.IsNullOrEmpty(tieude))
            {
                @ViewData["Loi1"] = "Vui lòng cho biết tiêu đề của bảng tin";
            }
            else if (String.IsNullOrEmpty(ngaydang))
            {
                @ViewData["Loi2"] = "Vui lòng cho biết ngày đăng của bảng tin";
            }
            else if (String.IsNullOrEmpty(noidung))
            {
                @ViewData["Loi3"] = "Vui lòng cho biết nội dung của bảng tin";
            }
            else if (String.IsNullOrEmpty(mota))
            {
                @ViewData["Loi4"] = "Vui lòng cho biết mô tả của bảng tin";
            }
            else if (fileupload == null)
            {
                tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
                tin.TieuDe = tieude;
                tin.NgayDang = DateTime.Parse(ngaydang);
                tin.MoTa = mota;
                tin.NoiDung = noidung;
                UpdateModel(tin);
                db.SubmitChanges();
                return RedirectToAction("TinTuc");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhTin"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Message = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
                    tin.TieuDe = tieude;
                    tin.NgayDang = DateTime.Parse(ngaydang);
                    tin.MoTa = mota;
                    tin.NoiDung = noidung;
                    tin.AnhBiaTin = fileName;
                    UpdateModel(tin);
                    db.SubmitChanges();
                    return RedirectToAction("TinTuc");
                }
            }
            return this.Edit(id);
        }

        public ActionResult Delete(int id)
        {
                BangTin tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
                ViewBag.LoaiTinID = new SelectList(db.LoaiTins.ToList().OrderBy(n => n.TenLoaiTin), "LoaiTinID", "TenLoaiTin", tin.LoaiTinID);
                ViewBag.AdminID = new SelectList(db.Admins.ToList().OrderBy(n => n.MaAdmin), "AdminID", "AdminID", tin.MaAdmin);
                if (tin == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }
                else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
                {
                    return RedirectToAction("Login", "Admin");
                }
                else
                    return View(tin);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult ConfirmOnDelete(int id)
        {
            BangTin tin = db.BangTins.SingleOrDefault(n => n.BangTinID == id);
            ViewBag.LoaiTinID = new SelectList(db.LoaiTins.ToList().OrderBy(n => n.TenLoaiTin), "LoaiTinID", "TenLoaiTin", tin.LoaiTinID);
            ViewBag.AdminID = new SelectList(db.Admins.ToList().OrderBy(n => n.MaAdmin), "MaAdmin", "AdminID", tin.MaAdmin);
            if (tin == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.BangTins.DeleteOnSubmit(tin);
            db.SubmitChanges();
            return RedirectToAction("TinTuc");
        }

        public ActionResult LoaiTin (int ? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(db.LoaiTins.ToList().OrderBy(n => n.LoaiTinID).ToPagedList(pageNum, pageSize)); 
        }

        public ActionResult CreateLoaiTin()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View();
        }

        [HttpPost, ActionName("CreateLoaiTin")]
        public ActionResult CreateLoaiTin(LoaiTin tin)
        {
            db.LoaiTins.InsertOnSubmit(tin);
            db.SubmitChanges();
            return RedirectToAction("LoaiTin");
        }

        public ActionResult EditLoaiTin(int id)
        {
            LoaiTin tin = db.LoaiTins.SingleOrDefault(n => n.LoaiTinID == id);
            if (tin == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(tin);
        }

        [HttpPost, ActionName("EditLoaiTin")]
        [ValidateInput(false)]
        public ActionResult ConfirmOnEditLoaiTin(int id, LoaiTin tin)
        {
            if (ModelState.IsValid)
            {
                tin = db.LoaiTins.SingleOrDefault(n => n.LoaiTinID == id);
                UpdateModel(tin);
                db.SubmitChanges();
            }
                return RedirectToAction("LoaiTin");
        }

        public ActionResult DeleteLoaiTin(int id)
        {
            LoaiTin tin = db.LoaiTins.SingleOrDefault(n => n.LoaiTinID == id);
            if (tin == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(tin);
        }

        [HttpPost, ActionName("DeleteLoaiTin")]
        public ActionResult ConfirmOnDeleteLoaiTin(int id)
        {
            LoaiTin tin = db.LoaiTins.SingleOrDefault(n => n.LoaiTinID == id);
            if (tin == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.LoaiTins.DeleteOnSubmit(tin);
            db.SubmitChanges();
            return RedirectToAction("LoaiTin");
        }

        public ActionResult HinhAnh(int ? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(db.HinhAnhs.ToList().OrderBy(n => n.NgayDangHinh).ToPagedList(pageNum, pageSize));
        }

        public ActionResult CreateHinhAnh()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateHinhAnh(HinhAnh hinh, HttpPostedFileBase fileupload, FormCollection collection)
        {
            var ngaydang = collection["NgayDangHinh"];
            var tuade = collection["TuaDe"];
            if (fileupload == null)
            {
                ViewData["Loi3"] = "Vui lòng chọn 1 tấm hình";
            }
            else if (String.IsNullOrEmpty(ngaydang))
            {
                ViewData["Loi1"] = "Ngày đăng hình thường là hôm nay";
            }
            else if (String.IsNullOrEmpty(tuade))
            {
                ViewData["Loi2"] = "Tựa đề của bức ảnh này là gì ?";
            }

            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhAnh"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Message = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    hinh.UrlHinh = fileName;
                    //hinh.TuaDe = tuade;
                    //hinh.NgayDangHinh = DateTime.Parse(ngaydang);
                    db.HinhAnhs.InsertOnSubmit(hinh);
                    db.SubmitChanges();
                    return RedirectToAction("HinhAnh");
                }
            }
            return this.CreateHinhAnh();
        }

        public ActionResult EditHinhAnh(int id)
        {
            HinhAnh tin = db.HinhAnhs.SingleOrDefault(n => n.HinhAnhID == id);
            if (tin == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(tin);
        }

        [HttpPost, ActionName("EditHinhAnh")]
        [ValidateInput(false)]
        public ActionResult ConfirmOnEditHinhAnh(int id, HinhAnh hinh, HttpPostedFileBase fileupload, FormCollection collection)
        {
            var ngaydang = collection["NgayDangHinh"];
            var tuade = collection["TuaDe"];
            if (String.IsNullOrEmpty(ngaydang))
            {
                @ViewData["Loi1"] = "Vui lòng chọn ngày đăng";
            }
            else if (String.IsNullOrEmpty(tuade))
            {
                @ViewData["Loi2"] = "Vui lòng nhập tựa đề của bức ảnh";
            }
            else if (fileupload == null)
            {
                hinh = db.HinhAnhs.SingleOrDefault(n => n.HinhAnhID == id);
                hinh.TuaDe = tuade;
                hinh.NgayDangHinh = DateTime.Parse(ngaydang);
                UpdateModel(hinh);
                db.SubmitChanges();
                return RedirectToAction("HinhAnh");
            }
            else 
            { 
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhAnh"), fileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Message = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    hinh = db.HinhAnhs.SingleOrDefault(n => n.HinhAnhID == id);
                    hinh.UrlHinh = fileName;
                    hinh.TuaDe = tuade;
                    hinh.NgayDangHinh = DateTime.Parse(ngaydang);
                    UpdateModel(hinh);
                    db.SubmitChanges();
                    return RedirectToAction("HinhAnh");
                }
            }
            return this.EditHinhAnh(id);
        }

        public ActionResult DeleteHinhAnh(int id)
        {
            HinhAnh hinh = db.HinhAnhs.SingleOrDefault(n => n.HinhAnhID == id);
            if (hinh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(hinh);
        }

        [HttpPost, ActionName("DeleteHinhAnh")]
        public ActionResult ConfirmOnDeleteHinhAnh(int id)
        {
            HinhAnh hinh = db.HinhAnhs.SingleOrDefault(n => n.HinhAnhID == id);
            if (hinh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.HinhAnhs.DeleteOnSubmit(hinh);
            db.SubmitChanges();
            return RedirectToAction("HinhAnh");
        }

        public ActionResult Admin(int? page)
        {
            var am = from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin;
            int pageNum = (page ?? 1);
            int pageSize = 10;
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View((from b in db.Admins where b.MaAdmin != (from a in db.Admins where a.MaAdmin == am.First() select a.MaAdmin).First() select b).ToList().OrderByDescending(n => n.MaAdmin).ToPagedList(pageNum, pageSize));
        }

        public ActionResult EditAdmin()
        {
            var am = from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin;
            Admin ad = db.Admins.SingleOrDefault(n => n.MaAdmin == am.First());
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(ad);
        }

        [HttpPost, ActionName("EditAdmin")]
        [ValidateInput(false)]
        public ActionResult ConfirmOnEditEditAdmin(Admin ad, FormCollection collection)
        {
            var am = from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin;
            if (ModelState.IsValid)
            {
                ad = db.Admins.SingleOrDefault(n => n.MaAdmin == am.First());
                UpdateModel(ad);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }
            return this.EditAdmin();
        }

        public ActionResult DeleteAdmin(int id)
        {
            var am = from admin in db.Admins where admin.AdminID.ToString() == Session["Username"].ToString() select admin.MaAdmin;
            Admin ad = db.Admins.SingleOrDefault(n => n.MaAdmin == id);
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(ad);
        }

        [HttpPost, ActionName("DeleteAdmin")]
        public ActionResult ConfirmOnDeleteAdmin(int id)
        {
            Admin ad = db.Admins.SingleOrDefault(n => n.MaAdmin == id);
            if (ad == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            { 
                db.Admins.DeleteOnSubmit(ad);
                db.SubmitChanges();
                return RedirectToAction("Admin");
            }
        }

        public ActionResult QuangCao(int ? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(db.QuangCaos.ToList().OrderBy(n => n.ViTri).ToPagedList(pageNum, pageSize));
        }

        public ActionResult CreateQuangCao(QuangCao qc)
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View();
        }

        [HttpPost, ActionName("CreateQuangCao")]
        public ActionResult ConfirmOnCreateQC(QuangCao qc)
        {
            db.QuangCaos.InsertOnSubmit(qc);
            db.SubmitChanges();
            return RedirectToAction("QuangCao");
        }

        public ActionResult EditQuangCao(int id)
        {
            QuangCao qc = db.QuangCaos.SingleOrDefault(n => n.QuangCaoID == id);
            if (qc == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(qc);
        }

        [HttpPost, ActionName("EditQuangCao")]
        [ValidateInput(false)]
        public ActionResult ConfirmOnEditQuangCao(int id, QuangCao qc)
        {
            if (ModelState.IsValid)
            {
                qc = db.QuangCaos.SingleOrDefault(n => n.QuangCaoID == id);
                UpdateModel(qc);
                db.SubmitChanges();
            }
            return RedirectToAction("QuangCao");
        }

        public ActionResult DeleteQuangCao(int id)
        {
            QuangCao qc = db.QuangCaos.SingleOrDefault(n => n.QuangCaoID == id);
            if (qc == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
                return View(qc);
        }

        [HttpPost, ActionName("DeleteQuangCao")]
        public ActionResult ConfirmOnDeleteQuangCao(int id)
        {
            QuangCao qc = db.QuangCaos.SingleOrDefault(n => n.QuangCaoID == id);
            if (qc == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.QuangCaos.DeleteOnSubmit(qc);
            db.SubmitChanges();
            return RedirectToAction("QuangCao");
        }

        public ActionResult AdminAccount()
        {
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                Admin admin = db.Admins.SingleOrDefault(n => n.AdminID == @Session["Username"]);
                return View(admin);
            }
        }

        public ActionResult CreateAdmin()
        {
            List<SelectListItem> myList = new List<SelectListItem>();
            myList.Add(new SelectListItem { Text = "Nam", Value = "Nam" });
            myList.Add(new SelectListItem { Text = "Nữ", Value = "Nữ" });
            ViewBag.data = myList;
            if (Session["Taikhoanadmin"] == null || Session["Taikhoanadmin"].ToString() == "")
                {
                    return RedirectToAction("Login", "Admin");
                }
            else
                return View();
        }

        [HttpPost, ActionName("CreateAdmin")]
        public ActionResult CreateLoaiTin(Admin ad)
        {
            db.Admins.InsertOnSubmit(ad);
            db.SubmitChanges();
            return RedirectToAction("Admin");
        }
    }
}