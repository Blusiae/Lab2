using System.Numerics;

namespace Encryption;

public class PolynomialsCalculator
{
    private readonly int _modPoly; // nieredukowalny wielomian (np. 0x11B)
    private readonly List<int> _poly = new List<int>();

    public PolynomialsCalculator(int poly)
    {
        if (poly <= 0) throw new ArgumentException("Poly must be > 0", nameof(poly));

        _modPoly = poly & 0x1FF; // zachowujemy ewentualne 9 bitów (np. 0x11B)
        _poly.Add(_modPoly);

        int degree = Degree(poly);
        if (degree <= 0) return;

        int maxValue = 1 << degree;
        int cur = 1;
        // precompute kolejne mnożenia przez x (czyli <<1 z redukcją)
        for (int i = 0; i < degree; i++)
        {
            int next = cur << 1;
            if ((next & maxValue) != 0)
            {
                next ^= _modPoly;
            }
            next &= (maxValue - 1);
            _poly.Add(next);
            cur = next;
        }
    }

    // Bezpieczne mnożenie w GF(2^8) (Russian peasant) z redukcją przez _modPoly
    public int Multiply(int a, int b)
    {
        a &= 0xFF;
        b &= 0xFF;

        int result = 0;
        while (b != 0)
        {
            if ((b & 1) != 0)
                result ^= a;

            b >>= 1;

            bool carry = (a & 0x80) != 0;
            a <<= 1;
            if (carry)
                a ^= _modPoly;

            a &= 0xFF;
        }

        return result & 0xFF;
    }

    // Szuka odwrotności (przez listę elementów pola lub brute-force)
    public int FindReverse(int polynomial, IEnumerable<int> polynomialsGalois)
    {
        polynomial &= 0xFF;
        if (polynomial == 0) return 0;

        if (polynomialsGalois != null)
        {
            foreach (var candidate in polynomialsGalois)
            {
                int c = candidate & 0xFF;
                if (c != 0 && Multiply(polynomial, c) == 1) return c;
            }
        }

        for (int x = 1; x < 256; x++)
        {
            if (Multiply(polynomial, x) == 1) return x;
        }

        return 0;
    }

    public static int Degree(int poly)
    {
        return poly == 0 ? -1 : BitOperations.Log2((uint)poly);
    }
}