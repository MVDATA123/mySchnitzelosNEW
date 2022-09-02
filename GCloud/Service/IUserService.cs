using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Service
{
    public interface IUserService : IAbstractService<User>
    {
        User FindbyUsername(string username);
    }
}