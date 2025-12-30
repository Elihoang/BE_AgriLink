using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class WorkerRepository : BaseRepository<Worker>, IWorkerRepository
{
    public WorkerRepository(ApplicationDbContext context) : base(context)
    {
    }
}
