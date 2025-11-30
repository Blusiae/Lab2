using Encryption;

if((args[0] == "encrypt" && args.Length != 2) || (args[0] == "decrypt" && args.Length != 6) || (args[0] != "encrypt" && args[0] != "decrypt"))
{
    Console.WriteLine("Usage:");
    Console.WriteLine("For encryption: dotnet run encrypt <file_path>");
    Console.WriteLine("For decryption: dotnet run decrypt <file_path> <output_path> <a> <b> <poly>");
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

var content = File.ReadAllBytes(filePath);
Random rd = new();
// generuj wielomiany dla GF(2^8)
int degree = 8;
var coefficients = IrreducibleFinder.GenerateCoefficients(degree).OrderBy(x => rd.Next()).ToList();
if (coefficients.Count == 0)
{
    Console.WriteLine("Brak wygenerowanych wielomianów.");
    return;
}

// wybierz wielomian nieredukowalny jako wielomian redukujący (pierwszy z listy)

int irreducible = args[0] == "decrypt" ? 
    int.TryParse(args[5], out int poly) ? poly : throw new ArgumentException("Niepoprawny wielomian redukujący.")
    : coefficients.First(x => IrreducibleFinder.IsIrreducible(x, degree));
var polyCalc = new PolynomialsCalculator(irreducible);

var crypter = new Crypter();

if(args[0] == "decrypt")
{
    if(!int.TryParse(args[3], out int a) || !int.TryParse(args[4], out int b))
    {
        Console.WriteLine("Niepoprawne wartości a lub b.");
        return;
    }   

    var decrypted = crypter.Decipher(content, a, b, polyCalc, coefficients);
    string decPath = args[2];
    await File.WriteAllBytesAsync(decPath, decrypted);
    Console.WriteLine($"Plik odszyfrowany: {decPath} (p={irreducible}, a={a}, b={b})");
    return;
}
else if(args[0] == "encrypt")
{
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
    var encrypted = crypter.Cipher(content, a, b, polyCalc);
    string outPath = filePath + ".enc";
    await File.WriteAllBytesAsync(outPath, encrypted);
    Console.WriteLine($"Plik zaszyfrowany: {outPath} (p={irreducible}, a={a}, b={b})");
}