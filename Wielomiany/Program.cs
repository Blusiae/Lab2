using System.Numerics;

int degree = 8;
var coefficients = GenerateCoefficients(degree);

coefficients
   .Where(c => IsIrreducible(c, degree))
   .ToList()
   .ForEach(c => Console.WriteLine(Convert.ToString(c, 2)));

static IEnumerable<int> GenerateCoefficients(int degree)
{
    int start = 1 << degree;
    int count = 1 << (degree + 1);

    return Enumerable.Range(start, count - start);
}

static bool IsIrreducible(int poly, int degree)
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
static int Mod(int dividend, int divisor)
{
    int degDiv = Degree(divisor);
    int degRem = Degree(dividend);

    while (degRem >= degDiv)
    {
        dividend ^= divisor << (degRem - degDiv);
        degRem = Degree(dividend);
    }
    return dividend;
}

// Stopień wielomianu (najwyższy bit)
static int Degree(int poly)
{
    return poly == 0 ? -1 : BitOperations.Log2((uint)poly);
}