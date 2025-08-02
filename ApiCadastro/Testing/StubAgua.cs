using NUnit.Framework;

public interface IWeatherService
{
    string GetWeatherForecast(string city);
}

public class WeatherApp
{
    private readonly IWeatherService _weatherService;

    public WeatherApp(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public string GetForecast(string city)
    {
        return _weatherService.GetWeatherForecast(city);
    }
}
public class WeatherServiceStub : IWeatherService
{
    public string GetWeatherForecast(string city)
    {
        // Retornando um resultado fixo para simplificar os testes
        return city == "São Paulo" ? "Sunny" : "Unknown";
    }
}

[TestFixture]
public class WeatherAppTests
{
    [Test]
    public void GetForecast_ReturnsSunny_WhenCityIsSaoPaulo()
    {
        // Arrange
        var weatherServiceStub = new WeatherServiceStub();
        var weatherApp = new WeatherApp(weatherServiceStub);

        // Act
        var result = weatherApp.GetForecast("São Paulo");

        // Assert
        Assert.AreEqual("Sunny", result);
    }

    [Test]
    public void GetForecast_ReturnsUnknown_WhenCityIsNotSaoPaulo()
    {
        // Arrange
        var weatherServiceStub = new WeatherServiceStub();
        var weatherApp = new WeatherApp(weatherServiceStub);

        // Act
        var result = weatherApp.GetForecast("Rio de Janeiro");

        // Assert
        Assert.AreEqual("Unknown", result);
    }
}