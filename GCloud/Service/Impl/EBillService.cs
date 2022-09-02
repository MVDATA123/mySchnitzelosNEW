using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Service.Impl
{
    public class BillService : AbstractService<Bill>, IBillService
    {
        public BillService(IBillRepository repository) : base(repository)
        {
        }
    }
}