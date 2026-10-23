using Microsoft.AspNetCore.Mvc;
using VetClinic.Contracts.Dto.Owners;
using VetClinic.Contracts.Services;

namespace VetClinic.Api.Controllers;

/// <summary>
/// Владельцы питомцев
/// </summary>
/// <param name="service">Сервис</param>
/// <param name="logger">Логгер</param>
[Route("api/owners")]
public class OwnersController(IOwnerService service, ILogger<OwnersController> logger)
    : CrudControllerBase<OwnerDto, OwnerEditDto>(service, logger)
{
    /// <inheritdoc/>
    protected override int GetId(OwnerDto dto) => dto.Id;
}
