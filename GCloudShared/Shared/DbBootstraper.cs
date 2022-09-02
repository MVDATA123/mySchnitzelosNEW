using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCloudShared.Domain;
using SQLite;
using Path = System.IO.Path;

namespace GCloudShared.Shared
{
    public class DbBootstraper
    {
        private static SQLiteConnection _connection;

        private static readonly Lazy<SQLiteConnection> SqLiteConnection = new Lazy<SQLiteConnection>(() =>
        {
            if (_connection == null)
            {
                var path = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                path = Path.Combine(path, "gcloud.db3");
                _connection = new SQLiteConnection(path);
                CreateAllTables(_connection);
            }

            return _connection;
        });

        public static SQLiteConnection Connection => SqLiteConnection.Value;

        private static void DeleteAll()
        {
            var subclasses = typeof(BasePersistable).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BasePersistable)));

            foreach (var subclass in subclasses)
            {
                Connection.DeleteAll(new TableMapping(subclass));
            }
        }

        private static void CreateAllTables()
        {
            var subclasses = typeof(BasePersistable).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BasePersistable)));

            foreach (var subclass in subclasses)
            {
                Connection.CreateTable(subclass);
            }
        }

        private static void CreateAllTables(SQLiteConnection connection)
        {
            var subclasses = typeof(BasePersistable).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(BasePersistable)));

            foreach (var subclass in subclasses)
            {
                connection.CreateTable(subclass);
            }
        }
    }
}
