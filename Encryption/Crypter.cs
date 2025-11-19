namespace Encryption;

public class Crypter
{
      // Encrypt using GF(2^8): E(x) = a * x + b  (where + is XOR, * is GF multiply)
    public byte[] Cipher(byte[] input, int a, int b, PolynomialsCalculator polyCalc)
    {
        ArgumentNullException.ThrowIfNull(polyCalc);
        byte[] output = new byte[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            int mul = polyCalc.Multiply(a, input[i]); // GF multiplication
            output[i] = (byte)(mul ^ b); // GF addition = XOR
        }

        return output;
    }

    // Decrypt: x = a_inv * (y + b)  (y + b is XOR)
    public byte[] Decipher(byte[] input, int a, int b, PolynomialsCalculator polyCalc, IEnumerable<int> fieldElements)
    {
        ArgumentNullException.ThrowIfNull(polyCalc);
        ArgumentNullException.ThrowIfNull(fieldElements);

        byte[] output = new byte[input.Length];

        int a_inv = polyCalc.FindReverse(a, fieldElements);
        if (a_inv == 0) throw new ArgumentException("Brak odwrotności dla podanego a w GF(2^8)");

        for (int i = 0; i < input.Length; i++)
        {
            int t = input[i] ^ b;               // y + b  (XOR)
            int plain = polyCalc.Multiply(a_inv, t); // a_inv * (y+b) in GF
            output[i] = (byte)plain;
        }

        return output;
    }
}
