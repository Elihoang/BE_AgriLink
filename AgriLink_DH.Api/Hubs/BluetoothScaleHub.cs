using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.DTOs.HarvestBagDetail;
using Microsoft.AspNetCore.SignalR;

namespace AgriLink_DH.Api.Hubs;

public class BluetoothScaleHub : Hub
{
    private readonly HarvestBagDetailService _service;
    private readonly ILogger<BluetoothScaleHub> _logger;

    // Cache to hold the last few weights for stabilization logic
    private static readonly Dictionary<string, List<decimal>> _weightStreams = new();
    private static readonly object _lock = new();

    public BluetoothScaleHub(HarvestBagDetailService service, ILogger<BluetoothScaleHub> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// App calls this continuously to push raw weight from scale
    /// </summary>
    public async Task StreamRawWeight(Guid sessionId, string scaleDeviceId, decimal currentWeight, int bagIndex, decimal deduction)
    {
        bool isStable = false;

        lock (_lock)
        {
            if (!_weightStreams.ContainsKey(scaleDeviceId))
            {
                _weightStreams[scaleDeviceId] = new List<decimal>();
            }

            var stream = _weightStreams[scaleDeviceId];
            stream.Add(currentWeight);

            // Keep only the last 5 readings (Assuming app sends 5 readings per second)
            if (stream.Count > 5)
            {
                stream.RemoveAt(0);
            }

            // Logic to determine stability: 
            // If we have 5 readings and the difference between max and min is <= 0.05
            // and the weight is > 5kg (ignore empty scale)
            if (stream.Count == 5 && currentWeight > 5m)
            {
                var max = stream.Max();
                var min = stream.Min();

                if (max - min <= 0.05m)
                {
                    isStable = true;
                    // Clear stream so we don't double trigger
                    _weightStreams.Remove(scaleDeviceId);
                }
            }
        }

        if (isStable)
        {
            _logger.LogInformation("Weight {weight} stabilized for scale {scaleDeviceId}. Saving to DB...", currentWeight, scaleDeviceId);

            try
            {
                var dto = new CreateHarvestBagDetailDto
                {
                    SessionId = sessionId,
                    BagIndex = bagIndex,
                    GrossWeight = currentWeight,
                    Deduction = deduction,
                    IsAutoWeighed = true,
                    ScaleDeviceId = scaleDeviceId
                };

                var newBag = await _service.AddBagAsync(dto);

                // Send success event back to the app that called this
                await Clients.Caller.SendAsync("OnBagLocked", newBag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save auto-weighed bag for session {sessionId}", sessionId);
                await Clients.Caller.SendAsync("OnError", "Không thể lưu số cẩn tự động: " + ex.Message);
            }
        }
    }
}
