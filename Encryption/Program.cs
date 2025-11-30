using System.Security.Cryptography;
using Encryption;

if((args[0] == "encrypt" && args.Length != 2) || (args[0] == "decrypt" && args.Length != 3) || (args[0] != "encrypt" && args[0] != "decrypt"))
{
    Console.WriteLine("Usage:");
    Console.WriteLine("For encryption: dotnet run encrypt <file_path>");
    Console.WriteLine("For decryption: dotnet run decrypt <file_path> <output_path>");
    return;
}

var filePath = args[1];
if (filePath is null)
    throw new ArgumentNullException(nameof(filePath));

if (!File.Exists(filePath))
{
    Console.WriteLine("Plik nie istnieje.");
    return;
}


Random rd = new();
int degree = 8;
var coefficients = IrreducibleFinder.GenerateCoefficients(degree).OrderBy(x => rd.Next()).ToList();
if (coefficients.Count == 0)
{
    Console.WriteLine("Brak wygenerowanych wielomianów.");
    return;
}

var crypter = new Crypter();

if(args[0] == "decrypt")
{
    await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
    byte[] seed = new byte[16];
    await fs.ReadExactlyAsync(seed);
    int a = seed[4];
    int b = seed[8];
    int irreducible = seed[12];
    byte[] content = new byte[fs.Length - 16];
    await fs.ReadAsync(content);

    var polyCalc = new PolynomialsCalculator(irreducible);

    var decrypted = crypter.Decipher(content, a, b, polyCalc, coefficients);
    string decPath = args[2];

    await File.WriteAllBytesAsync(decPath, decrypted);
    Console.WriteLine($"Plik odszyfrowany: {decPath}.");
    return;
}
else if(args[0] == "encrypt")
{
    int irreducible =  coefficients.First(x => IrreducibleFinder.IsIrreducible(x, degree));
    var polyCalc = new PolynomialsCalculator(irreducible);
    int a = 0, b = 0;
    foreach (var candidate in coefficients)
    {
        if (candidate == 0 || candidate == 1) continue;
        int inv = polyCalc.FindReverse(candidate, coefficients);
        if (inv != 0) { a = candidate; b = inv; break; }
    }

    if (a == 0 || a == 1 || b == 0)
    {
        Console.WriteLine("Nie znaleziono dopasowanych wartości a i b.");
        return;
    }

    var content = File.ReadAllBytes(filePath);
    var encrypted = crypter.Cipher(content, a, b, polyCalc);
    string outPath = filePath + ".enc";

    await using var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write);
    byte[] salt = new byte[16];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(salt);
    salt[4] = (byte)a;
    salt[8] = (byte)b;
    salt[12] = (byte)irreducible;
    await fs.WriteAsync(salt);
    await fs.WriteAsync(encrypted); 

    Console.WriteLine($"Plik zaszyfrowany: {outPath}.");
}