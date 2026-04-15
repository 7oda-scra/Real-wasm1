using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealDesign2.Server.Services;
using RealDesign2.Shared.Models;

namespace RealDesign2.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ModulesController : ControllerBase
{
    private readonly MockModuleCatalogService _moduleCatalog;

    public ModulesController(MockModuleCatalogService moduleCatalog)
    {
        _moduleCatalog = moduleCatalog;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ModuleDefinitionDto>> GetAllowedModules()
    {
        var allowedModuleIds = User.FindAll("AllowedModuleId").Select(claim => claim.Value);
        return Ok(_moduleCatalog.GetAllowedModules(allowedModuleIds));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public ActionResult<IReadOnlyList<ModuleDefinitionDto>> GetAllModules()
    {
        return Ok(_moduleCatalog.GetAllModules());
    }
}
