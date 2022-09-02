using GCloudShared.Domain;
using SQLite;
using System;

namespace GCloudShared.Repository
{
    public class StoreWhitelistRepository : AbstractRepository<StoreWhitelist>
    {
        public StoreWhitelistRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public int DeleteEntry(Guid storeId, string userId)
        {
            var whitelistEntry = FindFirstBy(entry => entry.Store.Equals(storeId) && entry.User.Equals(userId));
            if (whitelistEntry != null)
            {
                return _connection.Delete(whitelistEntry);
            }
            else
            {
                return 0;
            }
        }

        public void InsertIfNotExists(StoreWhitelist whitelistEntry)
        {
            var existingEntry = FindFirstBy(entry => entry.Store.Equals(whitelistEntry.Store) && entry.User.Equals(whitelistEntry.User));
            if(existingEntry != null)
            {
                return;
            }

            Insert(whitelistEntry);
        }
    }
}
