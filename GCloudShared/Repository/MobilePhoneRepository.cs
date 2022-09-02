using System;
using System.Collections.Generic;
using System.Text;
using GCloudShared.Domain;
using SQLite;

namespace GCloudShared.Repository
{
    public class MobilePhoneRepository : AbstractRepository<MobilePhone>
    {
        public MobilePhoneRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public override int Insert(MobilePhone entity)
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
