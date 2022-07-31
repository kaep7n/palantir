namespace Palantir;

public class HomaticHttpClient
{
    private readonly HttpClient http;

    public HomaticHttpClient(HttpClient http)
    {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public async Task<Devices> GetDevicesAsync()
        => await http.GetFromJsonAsync<Devices>("device")
            .ConfigureAwait(false);

    public async Task<Device> GetDeviceAsync(string id)
        => await http.GetFromJsonAsync<Device>($"device/{id}")
            .ConfigureAwait(false);
}
