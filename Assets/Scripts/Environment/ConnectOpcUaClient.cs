using Expi40.Core.AasSemantics;
using Expi40.Core.CommunicationInterfaces.BasyxClient;
using Expi40.Core.CommunicationInterfaces.OpcUaClient;
using Expi40.Core.Models.ThingDescription.Affordance.Property;
using Expi40.Core.NotificationManagement;
using Expi40.Core.Unity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Logic to connect to an OPC UA client when invoked.
    /// </summary>
    public class ConnectOpcUaClient : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _username;
        [SerializeField]
        private TMP_InputField _password;
        [SerializeField]
        private Button _connect;

        /// <summary>
        /// Path to the OPC UA client configuration file.
        /// </summary>
#if !UNITY_EDITOR
        private readonly string _opcUaClientConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpcUaClientResources", "Client.Config.xml");
#else
        private readonly string _opcUaClientConfigFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
            "OpcUaClientResources"); // TODO >> make this file instead of folder and update OpcUaClient lib
#endif

        /// <summary>
        /// OPC UA Client certificate checker executable path.
        /// </summary>
        /// <remarks>
        /// Checker is used to validate the client certificate before establishing a connection.
        /// </remarks>
#if !UNITY_EDITOR
        private readonly string _opcUaClientCertificateChecker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpcUaClientResources", "Expi40.OpcUaClient.ClientCertificateChecker.exe");
#else
        private readonly string _opcUaClientCertificateChecker = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
            "OpcUaClientResources"); // TODO >> make this file instead of folder and update OpcUaClient lib
#endif

        /// <summary>
        /// Handles connection of OPC UA Client to OPC UA Server.
        /// </summary>
        public async void OnClick()
        {
            try
            {
                // Check if current application instance is EXPI40 SERVER
                if (AppBootstrapper.Instance.ApplicationInstanceType != ApplicationInstance.SERVER)
                {
                    throw new InvalidOperationException("OPC UA Client can only be connected from an EXPI40 SERVER application instance.");
                }

                // Check if configuration file exists
                if (!Directory.Exists(_opcUaClientConfigFile))
                {
                    throw new FileNotFoundException($"OPC UA Client configuration file not found at path: {_opcUaClientConfigFile}");
                }

                // Check if certificate checker executable exists
                if (!Directory.Exists(_opcUaClientCertificateChecker))
                {
                    throw new FileNotFoundException($"OPC UA Client certificate checker not found at path: {_opcUaClientCertificateChecker}");
                }

                // Check if username and password are provided
                if (string.IsNullOrWhiteSpace(_username.text) || string.IsNullOrWhiteSpace(_password.text))
                {
                    throw new ArgumentException("Username and password must be provided to connect to OPC UA Server.");
                }

                var basyxClient = AppBootstrapper.Instance.Services.GetRequiredService<IBasyxClient>();

                // TODO >> Create method in ControllerModel - GetAssetinterfaceBySemanticId(string semanticId)
                var opcUaServerInterfaceDescription = basyxClient.GetProductionLineModel().Controller.AssetInterfaces
                    .FirstOrDefault(ai => ai.SemanticIds.Contains(SubmodelElementSemantics.OpcUaClientServerInterface));

                // Create ConnectionAttributes for OPC UA Client
                var connectionAttributes = new ConnectionAttributes(
                    opcUaServerEndpoint: opcUaServerInterfaceDescription.BaseEndpoint,
                    applicationName: $"{AppBootstrapper.Instance.ApplicationInstanceType}-{AppBootstrapper.Instance.InstanceId}",
                    clientConfigPath: _opcUaClientConfigFile,
                    username: _username.text,
                    password: _password.text,
                    clientCertificateCheckerPath: _opcUaClientCertificateChecker
                    );

                // Get OPC UA Client service
                var opcUaClient = AppBootstrapper.Instance.Services.GetRequiredService<IOpcUaClient>();
                // Connect to OPC UA Server
                await opcUaClient.Connect(connectionAttributes);

                AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().AddNotification("Successfully connected to OPC UA Server.");

                // TODO >> Create method in AssetInterfaceDescriptionModel
                if (opcUaServerInterfaceDescription.MandatoryInteractionMetadata[InteractionMetadataType.properties]
                    .FirstOrDefault(af => af.SemanticIds.Contains(SubmodelElementSemantics.OpcUaClientServerInterfacePropertiesNewPhysicalProduct)) is not OpcUaClientServerPropertyAffordance newPhysProdProp)
                {
                    // TODO >> define specific exception for missing required semanticId in AssetInterfaceDescription
                    throw new InvalidDataException($"{nameof(SubmodelElementSemantics.OpcUaClientServerInterfacePropertiesNewPhysicalProduct)} is not defined in {nameof(opcUaServerInterfaceDescription.MandatoryInteractionMetadata)}");
                } 
                // Subscribe to new physical product event
                opcUaClient.SubscribeNewPhysicalProductMonitor(newPhysProdProp.NodeId);

                // Subscribe to all control OPC UA Nodes on the OPC UA Server
                // Get all LineParts from the production line model
                var lineParts = basyxClient.GetProductionLineModel().LineParts;
                foreach (var linePart in lineParts)
                {
                    var gameObjectName = linePart.IdShort;

                    // Get all ControlProperties of the LinePart
                    var controlProperties = linePart.ControlProperties;
                    foreach (var controlProperty in controlProperties)
                    {
                        var controlPropertyName = controlProperty.ControlProperty.IdShort;

                        // Get all AssetInterfaceDescriptions of the ControlProperty
                        var assetInterfaceDescriptions = controlProperty.AssetInterfaceReferences;

                        assetInterfaceDescriptions
                            .Where(aid => aid.Operations.Contains(AssetInterfaceOperation.observeproperty))
                            .ToList()
                            .ForEach(aid =>
                            {
                                if (aid is OpcUaClientServerPropertyAffordance opcUaClientServerPropertyAffordance)
                                {
                                    opcUaClient.SubscribeNode(opcUaClientServerPropertyAffordance.NodeId, gameObjectName, controlPropertyName);
                                }
                            });
                    }
                }

                await opcUaClient.ApplySubscriptions();

                _username.interactable = false;
                _password.interactable = false;
                _connect.enabled = false;
            }
            catch (Exception ex)
            {
                AppBootstrapper.Instance.Services.GetRequiredService<INotificationManager>().AddError(ex);
            }
        }
    }
}
