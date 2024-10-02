namespace OpenShock.Integrations.RiskOfRain2.Utils;

public class MathUtils
{
    public static float Lerp(float min, float max, float t) => min + (max - min) * t;
    public static float Saturate(float value) => value < 0 ? 0 : value > 1 ? 1 : value;
}