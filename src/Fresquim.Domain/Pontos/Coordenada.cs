using Fresquim.Domain.Common;

namespace Fresquim.Domain.Pontos;

/// <summary>Objeto de valor imutável com latitude e longitude.</summary>
public sealed record Coordenada
{
    private const double RaioTerraKm = 6371.0;

    public double Latitude { get; }
    public double Longitude { get; }

    public Coordenada(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90) throw new DomainException("Latitude deve estar entre -90 e 90.");
        if (longitude is < -180 or > 180) throw new DomainException("Longitude deve estar entre -180 e 180.");
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Distância em km até outra coordenada (fórmula de Haversine).</summary>
    public double DistanciaKm(Coordenada outra)
    {
        static double Rad(double graus) => graus * Math.PI / 180;

        var dLat = Rad(outra.Latitude - Latitude);
        var dLon = Rad(outra.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(Rad(Latitude)) * Math.Cos(Rad(outra.Latitude)) * Math.Pow(Math.Sin(dLon / 2), 2);

        return RaioTerraKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
