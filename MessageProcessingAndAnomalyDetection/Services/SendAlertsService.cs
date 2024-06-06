using MessageProcessingAndAnomalyDetection.Interfaces;
using MessageProcessingAndAnomalyDetection.Interfaces.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace MessageProcessingAndAnomalyDetection.Services;

public class SendAlertsService : ISendAlertsService
{
    private readonly IAnomalyDetectionService _anomalyDetectionService;
    private readonly IAnomalyDetectionConfigurations _anomalyDetectionConfigurations;
    private readonly HubConnection _hubConnection;

    public SendAlertsService(IAnomalyDetectionService anomalyDetectionService,
        IAnomalyDetectionConfigurations anomalyDetectionConfigurations,
        HubConnection hubConnection)
    {
        _anomalyDetectionService = anomalyDetectionService;
        _anomalyDetectionConfigurations = anomalyDetectionConfigurations;
        _hubConnection = hubConnection;
    }

    public void SendAlerts(IServerStatistics currentServerStatistics, IServerStatistics previousServerStatistics)
    {
        if (previousServerStatistics != null)
        {
            var alerts = _anomalyDetectionService.ServerAnomalyDetection(_anomalyDetectionConfigurations,
                previousServerStatistics,
                currentServerStatistics);

            if (alerts != null)
            {
                var alertsJson = JsonConvert.SerializeObject(alerts);

                _hubConnection.InvokeAsync("SendMessage", "Alerts", alertsJson);
            }
        }
    }
}