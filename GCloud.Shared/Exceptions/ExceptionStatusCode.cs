using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared.Exceptions
{
    public enum ExceptionStatusCode
    {
        UserNotFound,
        StoreNotFound,
        ApiTokenInvalid,
        CouponNotFound,
        CredentialsInvalid,
        UserDisabled,
        EmailNotConfirmed,
        UsernameAlreadyTaken,
        GeneralRegistrationException,
        SendMailException,
        OldPasswordInvalid,
        CashbackNotFound,
        NoLastCashback,
        AlreadyRedeemed,
        ArgumentInvalid,
        CashRegisterNotInStore
    }
}
