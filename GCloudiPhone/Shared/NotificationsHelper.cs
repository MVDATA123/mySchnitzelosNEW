using System;
using System.Linq;
using Firebase.CloudMessaging;
using GCloudShared.Domain;
using GCloudShared.Repository;
using GCloudShared.Shared;

namespace GCloudiPhone.Shared
{
    public class NotificationsHelper
    {
        private static NotificationsHelper _instance;
        private readonly StoreWhitelistRepository whitelistRepository;
        private readonly UserRepository userRepository;

        public static NotificationsHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NotificationsHelper();
                    return _instance;
                }
                return _instance;
            }
        }

        public NotificationsHelper()
        {
            whitelistRepository = new StoreWhitelistRepository(DbBootstraper.Connection);
            userRepository = new UserRepository(DbBootstraper.Connection);
        } 

        public void SubscribeStore(Guid storeId)
        {
            var userId = userRepository.GetCurrentUser()?.UserId;
            whitelistRepository.InsertIfNotExists(new StoreWhitelist { Store = storeId, User = userId });
            Messaging.SharedInstance.Subscribe(topic: storeId.ToString());
        }

        public void UnsubscribeStore(Guid storeId)
        {
            var userId = userRepository.GetCurrentUser()?.UserId;
            whitelistRepository.DeleteEntry(storeId, userId);
            Messaging.SharedInstance.Unsubscribe(topic: storeId.ToString());
        }

        /// <summary>
        /// Subscribes to all FCM topics that are in the whitelist
        /// </summary>
        public void SubscribeAll()
        {
            var userId = userRepository.FindAll().First().UserId;
            var subscriptions = whitelistRepository.FindBy(sub => sub.User.Equals(userId));
            foreach (var subscription in subscriptions)
            {
                Messaging.SharedInstance.Subscribe(topic: subscription.Store.ToString());
            }
        }

        /// <summary>
        /// Unsubscribes from all FCM topics but DOES NOT delete them from the whitelist
        /// </summary>
        public void UnsubscribeAll() {
            var userId = userRepository.FindAll().First().UserId;
            var subscriptions = whitelistRepository.FindBy(sub => sub.User.Equals(userId));
            foreach (var subscription in subscriptions)
            {
                Messaging.SharedInstance.Unsubscribe(topic: subscription.Store.ToString());
            }
        }
    }
}
