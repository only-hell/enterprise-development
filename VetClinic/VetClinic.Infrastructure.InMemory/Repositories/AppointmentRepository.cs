using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;
using VetClinic.Domain.Seeds;

namespace VetClinic.Infrastructure.InMemory.Repositories;

/// <summary>
/// Репозиторий записей в памяти
/// </summary>
/// <param name="seed">Начальные данные</param>
public class AppointmentRepository(VetClinicSeed seed) : InMemoryRepository<Appointment>(seed.Appointments), IAppointmentRepository
{
    /// <inheritdoc/>
    protected override int GetId(Appointment entity) => entity.Id;

    /// <inheritdoc/>
    protected override void SetId(Appointment entity, int id) => entity.Id = id;
}