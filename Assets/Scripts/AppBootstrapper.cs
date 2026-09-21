using Expi40.BasyxClient;
using Expi40.Core.CommunicationInterfaces.BasyxClient;
using Expi40.Core.CommunicationInterfaces.OpcUaClient;
using Expi40.Core.CommunicationInterfaces.OpcUaPubSub;
using Expi40.Core.EventManagement;
using Expi40.Core.NotificationManagement;
using Expi40.Core.Unity;
using Expi40.EventManagement;
using Expi40.NotificationManagement;
using Expi40.OpcUaClient;
using Expi40.OpcUaPubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Provides a singleton-based bootstrapper for initializing and managing application-wide services, configuration and data.
    /// </summary>
    public class AppBootstrapper : MonoBehaviour
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="AppBootstrapper"/> class.
        /// </summary>
        public static AppBootstrapper Instance { get; private set; }

        /// <summary>
        /// Gets the type of the application instance.
        /// </summary>
        public ApplicationInstance ApplicationInstanceType { get; private set; }

        /// <summary>
        /// Gets the type of build target.
        /// </summary>
        public BuildTargetType BuildTargetType { get; private set; }

        /// <summary>
        /// Unique identifier for this instance of the current application instance.
        /// </summary>
        /// <remarks>
        /// Used e.g. as ClientId for MQTT connections.
        /// </remarks>
        public Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Service provider for dependency injection.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Indicates whether the application is in the process of quitting.
        /// This can be used by other components to check if they should perform cleanup or other actions during application shutdown.
        /// </summary>
        public bool ApplicationQuitting { get; private set; } = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_STANDALONE_WIN && UNITY_EXPI40_SERVER
            ApplicationInstanceType = ApplicationInstance.SERVER;
#else
            ApplicationInstanceType = ApplicationInstance.CLIENT;
#endif

#if UNITY_STANDALONE_WIN
            BuildTargetType = BuildTargetType.UNITY_STANDALONE_WIN;
#elif UNITY_ANDROID
            BuildTargetType = BuildTargetType.UNITY_ANDROID;
#elif UNITY_WSA
            BuildTargetType = BuildTargetType.UNITY_WSA;
#else
            BuildTargetType = BuildTargetType.UNKNOWN;
#endif

            InitializeServices();
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            services.AddHttpClient();
            services.TryAddSingleton<IEventManager, EventManager>();
            services.TryAddSingleton<INotificationManager, NotificationManager>();
            services.TryAddSingleton<IBasyxClient, BasyxClient>();
            services.TryAddSingleton<ISubscriber, Subscriber>();
            services.TryAddSingleton<IPublisher, Publisher>();

#if UNITY_STANDALONE_WIN && UNITY_EXPI40_SERVER
            services.TryAddSingleton<IOpcUaClient, OpcUaClient>();
#endif

            Services = services.BuildServiceProvider();
        }

        void OnApplicationQuit()
        {
            ApplicationQuitting = true;

            if (Services is IDisposable d)
            {
                d.Dispose();
            }

            Services = null;
        }
    }
}