using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions.Cashier
{
    public class CashRegisterNotInStoreException : BaseCashierException
    {
        public CashRegisterNotInStoreException(Guid cashRegiserId) : base(cashRegiserId, ExceptionStatusCode.CashRegisterNotInStore, "Die Registrierkasse wurde dieser Filiale nicht zugewiesen!")
        {
        }
    }
}
