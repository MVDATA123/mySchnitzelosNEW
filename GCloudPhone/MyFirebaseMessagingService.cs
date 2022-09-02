using Android.App;
using Android.Content;
using Android.Util;
using Firebase.Messaging;
using System.Linq;

namespace mvdata.foodjet
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        private const int OpenActivityRequestCode = 1;

        private NotificationHelper _notificationHelper;
        private int _currentNotification = 0;
        private int _currentNotificationBill = 0;

        private const string TAG = "MyFirebaseMsgService";
        public override void OnMessageReceived(RemoteMessage message)
        {
            _notificationHelper = new NotificationHelper(this);
            Log.Debug(TAG, "From: " + message.From);
            Log.Debug(TAG, "Notification Message Body: " + string.Join(";", message.Data.Select(kvp => $"{kvp.Key} => {kvp.Value}")));

            if (message.Data.ContainsKey("body") && message.Data.ContainsKey("title"))
            {
                var title = message.Data["title"];
                var body = message.Data["body"];
                var billId = message.Data["billId"];

                if (message.Data.ContainsKey("type") && message.Data["type"] == NotificationHelper.BILL_CHANNEL)
                    SendNotificationBill(title, body, billId);
                else
                {
                    var storeGuid = message.Data["storeGuid"];
                    var storeName = message.Data["storeName"];
                    var cashbackEnabled = bool.Parse(message.Data["cashbackEnabled"]);
                    SendNotification(title, body, storeGuid, storeName, cashbackEnabled);
                }
            }
        }

        void SendNotification(string title, string messageBody, string storeGuid, string storeName, bool cashbackEnabled)
        {
            var intent = new Intent(this, typeof(CouponsListActivity));
            intent.PutExtra("storeGuid", storeGuid);
            intent.PutExtra("storeName", storeName);
            intent.PutExtra("storeCashbackEnabled", cashbackEnabled);
            intent.PutExtra("isUserAlreadyFollowing", true);
            var pendingIntent = PendingIntent.GetActivity(this, OpenActivityRequestCode, intent, PendingIntentFlags.UpdateCurrent);

            var summaryNotification = _notificationHelper.GetSummaryNotification($"{storeName} hat neue Angebote!", string.Empty);
            var notification = _notificationHelper.GetNotification(title, messageBody, pendingIntent);
            _notificationHelper.Notify(_currentNotification, notification);
            _notificationHelper.Notify(100, summaryNotification);
            _currentNotification++;
        }

        void SendNotificationBill(string title, string messageBody, string billId)
        {
            var intent = new Intent(this, typeof(BillDetailsActivity));
            intent.PutExtra("billGuid", billId);

            var pendingIntent = PendingIntent.GetActivity(this, OpenActivityRequestCode, intent, PendingIntentFlags.UpdateCurrent);

            var summaryNotification = _notificationHelper.GetSummaryNotification($"Rechnung", string.Empty, NotificationHelper.BILL_CHANNEL);
            var notification = _notificationHelper.GetNotification(title, messageBody, pendingIntent);
            _notificationHelper.Notify(_currentNotification, notification);
            _notificationHelper.Notify(10, summaryNotification);
            _currentNotificationBill++;
        }
    }
}