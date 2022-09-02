using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;

namespace mvdata.foodjet
{
    public class NotificationHelper : ContextWrapper
    {
        public const string PRIMARY_CHANNEL = "default";
        public const string SECONDARY_CHANNEL = "second";
        public const string BILL_CHANNEL = "bill";

        int SmallIcon => Android.Resource.Drawable.StatNotifyChat;

        NotificationManager manager;
        NotificationManager Manager
        {
            get
            {
                if (manager == null)
                {
                    manager = (NotificationManager)GetSystemService(NotificationService);
                }
                return manager;
            }
        }

        public NotificationHelper(Context context) : base(context)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                var primaryChannel =
                    new NotificationChannel(PRIMARY_CHANNEL, "Hauptnachrichten", NotificationImportance.Default)
                    {
                        LightColor = Color.Green,
                        LockscreenVisibility = NotificationVisibility.Private
                    };
                var billChannel =
                    new NotificationChannel(BILL_CHANNEL, "Rechnungen", NotificationImportance.High)
                    {
                        LightColor = Color.LightBlue,
                        LockscreenVisibility = NotificationVisibility.Private
                    };
                Manager.CreateNotificationChannel(primaryChannel);
                Manager.CreateNotificationChannel(billChannel);
            }
        }

        public Notification GetNotification(string title, string body, PendingIntent intent, string channel = PRIMARY_CHANNEL)
        {
            return new NotificationCompat.Builder(ApplicationContext, channel)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetSmallIcon(SmallIcon)
                .SetSound(null)
                .SetContentIntent(intent)
                .SetAutoCancel(true)
                .SetDefaults(0)
                .SetGroup(DateTime.Now.Date.ToString("yyyyMMdd"))
                .SetGroupAlertBehavior(NotificationCompat.GroupAlertSummary)
                .Build();
        }

        public Notification GetSummaryNotification(string title, string body, string channel = PRIMARY_CHANNEL)
        {
            return new NotificationCompat.Builder(ApplicationContext, channel)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetSmallIcon(SmallIcon)
                .SetGroup(DateTime.Now.Date.ToString("yyyyMMdd"))
                .SetAutoCancel(true)
                .SetGroupSummary(true)
                .SetGroupAlertBehavior(NotificationCompat.GroupAlertSummary)
                .Build();
        }

        public void Notify(int id, Notification notification)
        {
            Manager.Notify(id, notification);
        }
    }
}