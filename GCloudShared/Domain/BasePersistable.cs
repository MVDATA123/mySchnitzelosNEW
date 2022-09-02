using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace GCloudShared.Domain
{
    public abstract class BasePersistable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
