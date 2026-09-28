using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PluriOne.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public sealed class ErrorModel : PageModel
{
    public int Code { get; set; } = 500;
    public string Message { get; set; } = "Ocurrió un error inesperado.";

    public void OnGet(int? code = null)
    {
        if (code.HasValue)
        {
            Code = code.Value;
        }
        else
        {
            var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeFeature is not null)
            {
                Code = statusCodeFeature.OriginalStatusCode;
            }
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature?.Error is not null)
            {
                Code = 500;
            }
        }

        Message = Code switch
        {
            404 => "La página que buscas no existe.",
            403 => "No tienes permisos para ver este contenido.",
            _ => "Ocurrió un error inesperado."
        };
    }
}
