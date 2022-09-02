using System;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface IMobilePhoneService : IAbstractService<MobilePhone>
    {
        MobilePhone CreateNewDevice(string userId, string firebaseInstanceId);

        bool RemoveDevice(string userId, Guid deviceId);
    }
}