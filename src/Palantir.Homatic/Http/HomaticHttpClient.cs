using System.Net.Http.Json;

namespace Palantir.Homatic.Http;

public class HomaticHttpClient
{
    private readonly HttpClient http;

    public HomaticHttpClient(HttpClient http)
        => this.http = http ?? throw new ArgumentNullException(nameof(http));

    public async Task<Devices?> GetDevicesAsync()
        => await this.http.GetFromJsonAsync<Devices>("device")
            .ConfigureAwait(false);

    public async Task<Device?> GetDeviceAsync(string id)
        => await this.http.GetFromJsonAsync<Device>($"device/{id}")
            .ConfigureAwait(false);

    public async Task<Channel?> GetChannelAsync(string deviceId, string id)
        => await this.http.GetFromJsonAsync<Channel>($"device/{deviceId}/{id}")
            .ConfigureAwait(false);

    public async Task<Parameter?> GetParameterAsync(string deviceId, string channelId, string parameterId)
        => await this.http.GetFromJsonAsync<Parameter>($"device/{deviceId}/{channelId}/{parameterId}")
                .ConfigureAwait(false);
}