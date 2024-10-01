namespace OpenShock.Integrations.RiskOfRain2.Utils;

public class MathUtils
{
    public static float Lerp(float min, float max, float t) => min + (max - min) * t;
}