using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetKlinikSistemi.Models;
using VetKlinikSistemi.Models.ViewModels;
using VetKlinikSistemi.Services.Interfaces;

namespace VetKlinikSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly IKlinikServisi _klinikServisi;
        private readonly IHizmetServisi _hizmetServisi;
        private readonly IVeterinerServisi _veterinerServisi;

        public HomeController(
            IKlinikServisi klinikServisi,
            IHizmetServisi hizmetServisi,
            IVeterinerServisi veterinerServisi)
        {
            _klinikServisi = klinikServisi;
            _hizmetServisi = hizmetServisi;
            _veterinerServisi = veterinerServisi;
        }

        // Halka acik ana sayfa: klinikler, hizmetler ve veteriner kadrosu listelenir
        public async Task<IActionResult> Index()
        {
            var model = new VitrinViewModel
            {
                Klinikler = await _klinikServisi.TumKlinikleriGetirAsync(),
                Hizmetler = await _hizmetServisi.TumHizmetleriGetirAsync(),
                Veterinerler = await _veterinerServisi.TumVeterinerleriGetirAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}