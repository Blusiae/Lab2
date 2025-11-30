namespace Encryption;

public class Crypter
{
    public byte[] Cipher(byte[] input, int a, int b, PolynomialsCalculator polyCalc)
    {
        ArgumentNullException.ThrowIfNull(polyCalc);
        byte[] output = new byte[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            int mul = polyCalc.Multiply(a, input[i]);
            output[i] = (byte)(mul ^ b);
        }

        return output;
    }

    public byte[] Decipher(byte[] input, int a, int b, PolynomialsCalculator polyCalc, IEnumerable<int> fieldElements)
    {
        ArgumentNullException.ThrowIfNull(polyCalc);
        ArgumentNullException.ThrowIfNull(fieldElements);

        byte[] output = new byte[input.Length];

        int a_inv = polyCalc.FindReverse(a, fieldElements);
        if (a_inv == 0) throw new ArgumentException("Brak odwrotności dla podanego a w GF(2^8)");

        for (int i = 0; i < input.Length; i++)
        {
            int t = input[i] ^ b;
            int plain = polyCalc.Multiply(a_inv, t);
            output[i] = (byte)plain;
        }

        return output;
    }
}
