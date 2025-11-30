namespace Encryption;

public class IrreducibleFinder
{
    public static bool IsIrreducible(int poly, int degree)
    {
        for (int d = 1; d <= degree / 2; d++)
        {
            int start = 1 << d;
            int count = 1 << (d + 1);

            for (int factor = start; factor < count; factor++)
            {
                if (Mod(poly, factor) == 0)
                    return false; // rozkładalny
            }
        }
        return true; // nierozkładalny
    }

    // Dzielenie wielomianów nad GF(2) — zwraca resztę
    public static int Mod(int dividend, int divisor)
    {
        int degDiv = PolynomialsCalculator.Degree(divisor);
        int degRem = PolynomialsCalculator.Degree(dividend);

        while (degRem >= degDiv)
        {
            dividend ^= divisor << (degRem - degDiv);
            degRem = PolynomialsCalculator.Degree(dividend);
        }
        return dividend;
    }

    // Stopień wielomianu (najwyższy bit)


    public static IEnumerable<int> GenerateCoefficients(int degree)
    {
        int count = 1 << degree;

        return Enumerable.Range(1, count - 1);
    }
}
