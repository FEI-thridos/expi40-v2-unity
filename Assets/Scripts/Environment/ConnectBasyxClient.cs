using Expi40.Core.CommunicationInterfaces.BasyxClient;
using Expi40.Core.NotificationManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Logic to connect to a BaSyx client when invoked.
    /// </summary>
    public class ConnectBasyxClient : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _url;
        [SerializeField]
        private TMP_InputField _productionLineShortId;
        [SerializeField]
        private Button _connect;

        /// <summary>
        /// Handles connection of BaSyx Client to BaSyx Server.
        /// </summary>
        public async void OnClick()
        {
            try
            {
                // Check if url and production line short id are provided
                if (string.IsNullOrWhiteSpace(_url.text) || string.IsNullOrWhiteSpace(_productionLineShortId.text))
                {
                    throw new InvalidOperationException("URL and Production Line Short ID must be provided to connect the BaSyx Client.");
                }

                var basyxClient = AppBootstrapper.Instance.Services.GetRequiredService<IBasyxClient>();

                // TODO >> move to ConnectionAttributes
                await basyxClient.PrepareProductionLineModel(
                    _url.text,
                    _productionLineShortId.text,
                    AppBootstrapper.Instance.ApplicationInstanceType,
                    AppBootstrapper.Instance.BuildTargetType);

                AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().AddNotification("Successfully connected to BaSyx Client.");

                _url.interactable = false;
                _productionLineShortId.interactable = false;
                _connect.enabled = false;
            }
            catch (Exception ex)
            {
                AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().AddError(ex);
            }
        }
    }
}