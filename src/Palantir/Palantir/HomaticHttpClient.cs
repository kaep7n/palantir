namespace Palantir;

public class HomaticHttpClient
{
    private readonly HttpClient http;

    public HomaticHttpClient(HttpClient http)
    {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public async Task<Devices> GetDevicesAsync()
    {
        var devices = await http.GetFromJsonAsync<Devices>("device")
            .ConfigureAwait(false);

        return devices ?? new Devices(string.Empty, string.Empty, string.Empty, Array.Empty<Link>());
    }
}
