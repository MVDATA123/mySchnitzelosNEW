using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using Microsoft.AspNet.Identity;

namespace GCloud.Service.Impl
{
    public class UserService : AbstractService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) : base(userRepository)
        {
            this._userRepository = userRepository;
        }

        public User FindbyUsername(string username)
        {
            return _userRepository.FindFirstOrDefault(x => x.UserName == username);
        }
    }
}