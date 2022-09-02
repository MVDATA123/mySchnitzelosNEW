using System;
using GCloudShared.Domain;
using SQLite;

namespace GCloudShared.Repository
{
    public class LogRepository : AbstractRepository<LogMessage>
    {
        public LogRepository(SQLiteConnection connection) : base(connection)
        {
        }


    }
}
