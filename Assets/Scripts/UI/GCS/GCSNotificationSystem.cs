using System.Collections.Generic;
using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.UI.GCS
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Critical
    }

    public class NotificationCard
    {
        public string Title;
        public string Message;
        public NotificationType Type;
        public float Timestamp;
        public float Duration = 4.0f;
    }

    /// <summary>
    /// Ground Control Station Toast Notification Manager.
    /// Broadcasts and renders animated alert cards (Info, Success, Warning, Critical).
    /// </summary>
    public class GCSNotificationSystem : Singleton<GCSNotificationSystem>
    {
        private List<NotificationCard> activeNotifications = new List<NotificationCard>();

        public List<NotificationCard> ActiveNotifications => activeNotifications;

        private void Update()
        {
            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                if (Time.time - activeNotifications[i].Timestamp >= activeNotifications[i].Duration)
                {
                    activeNotifications.RemoveAt(i);
                }
            }
        }

        public void PostNotification(string title, string message, NotificationType type, float duration = 4.0f)
        {
            NotificationCard card = new NotificationCard
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = Time.time,
                Duration = duration
            };

            activeNotifications.Add(card);
            if (activeNotifications.Count > 5) activeNotifications.RemoveAt(0); // Max 5 visible cards

            Debug.Log($"[GCS Alert] [{type}] {title}: {message}", LogCategory.UI);
        }
    }
}


