namespace FieldOps.Api.Rag;

public static class VectorMath
{
    // 1.0 = same direction (same meaning), 0 = unrelated
    public static double Cosine(float[] a, float[] b)
    {
        double dot = 0, lengthA = 0, lengthB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            lengthA += a[i] * a[i];
            lengthB += b[i] * b[i];
        }
        return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));
    }
}