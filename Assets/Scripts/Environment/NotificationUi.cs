using Expi40.Core.EventManagement;
using Expi40.Core.NotificationManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    public class NotificationUi : MonoBehaviour
    {
        private enum NotificationsWindowState
        {
            Closed,
            Notifications,
            Errors
        }

        private NotificationsWindowState _notificationsWindowState;
        private Action<NotificationType> _onNotificationHandler;

        [SerializeField]
        private Button _notificationsButton;
        [SerializeField]
        private Button _errorsButton;
        [SerializeField]
        private Transform _notificationsWindow;
        [SerializeField]
        private TextMeshProUGUI _textContent;

        void Awake()
        {
            _onNotificationHandler = HandleNotification;
            SetNotifications();
            SetErrors();
        }

        void OnEnable()
        {
            AppBootstrapper.Instance.Services.GetRequiredService<IEventManager>().StartListening(Expi40.Core.EventManagement.EventType.OnNotification, _onNotificationHandler);
        }

        void OnDisable()
        {
            if (AppBootstrapper.Instance.ApplicationQuitting)
                return;

            AppBootstrapper.Instance.Services.GetRequiredService<IEventManager>().StopListening(Expi40.Core.EventManagement.EventType.OnNotification, _onNotificationHandler);
        }

        void Start()
        {
            _notificationsWindow.gameObject.SetActive(false);
            _notificationsWindowState = NotificationsWindowState.Closed;
        }

        private void HandleNotification(NotificationType notificationType)
        {
            switch (notificationType)
            {
                case NotificationType.Notification:
                    SetNotifications();
                    break;
                case NotificationType.Exception:
                    SetErrors();
                    break;
                default:
                    break;
            }
        }

        private int errorCount = -1;
        private void SetErrors()
        {
            // TODO >> Add Errors count to expi40-libs
            errorCount++;
            _errorsButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = $"{(errorCount < 100 ? errorCount : "99+")}";
            // TODO >> Add "-------------" split between messages in ErrorsAsString
            _textContent.text = AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().ErrorsAsString; // TODO 
        }

        private int notificationCount = -1;
        private void SetNotifications()
        {
            // TODO >> Add Notifications count to expi40-libs
            notificationCount++;
            _notificationsButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{(notificationCount < 100 ? notificationCount : "99+")}";
            // TODO >> Add "-------------" split between messages in NotificationsAsString
            _textContent.text = AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().NotificationsAsString;
        }

        public void OnErrorsButtonClick()
        {
            if (!_notificationsWindow.gameObject.activeInHierarchy)
            {
                _textContent.text = AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().ErrorsAsString;
                _notificationsWindow.gameObject.SetActive(true);
                _notificationsWindowState = NotificationsWindowState.Errors;
            }
            else
            {
                if (_notificationsWindowState == NotificationsWindowState.Errors)
                {
                    _notificationsWindow.gameObject.SetActive(false);
                    _notificationsWindowState = NotificationsWindowState.Closed;
                }
                else
                {
                    _textContent.text = AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().ErrorsAsString;
                    _notificationsWindowState = NotificationsWindowState.Errors;
                }
            }
        }

        public void OnNotificationsButtonClick()
        {
            if (!_notificationsWindow.gameObject.activeInHierarchy)
            {
                _textContent.text = AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().NotificationsAsString;
                _notificationsWindow.gameObject.SetActive(true);
                _notificationsWindowState = NotificationsWindowState.Notifications;
            }
            else
            {
                if (_notificationsWindowState == NotificationsWindowState.Notifications)
                {
                    _notificationsWindow.gameObject.SetActive(false);
                    _notificationsWindowState = NotificationsWindowState.Closed;
                }
                else
                {
                    _textContent.text = AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().NotificationsAsString;
                    _notificationsWindowState = NotificationsWindowState.Notifications;
                }
            }
        }
    }
}
