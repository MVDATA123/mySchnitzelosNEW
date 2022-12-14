using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCloudShared.Domain;
using SQLite;

namespace GCloudShared.Repository
{
    public sealed class UserRepository : AbstractRepository<User>
    {
        public UserRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public User GetCurrentUser()
        {
            var usercount = Count();

            return usercount > 0 ? FindAll().First() : null;
        }

        public override int Insert(User entity)
        {
            var current = Count();

            if (current > 0)
            {
                DeleteAll();
            }
            return base.Insert(entity);
        }
    }
}
