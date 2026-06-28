using Microsoft.AspNetCore.Mvc;

namespace NutriIndex.Ingestion.Controllers;

public class IngestionController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
