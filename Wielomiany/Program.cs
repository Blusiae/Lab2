using Wielomiany;

var filePath = Console.ReadLine();
if (filePath is null)
    throw new ArgumentNullException(nameof(filePath));

if (!File.Exists(filePath))
{
    Console.WriteLine("Plik nie istnieje.");
    return;
}

var content = File.ReadAllBytes(filePath);

// generuj wielomiany dla GF(2^8)
int degree = 8;
var coefficients = IrreducibleFinder.GenerateCoefficients(degree).ToList();
if (coefficients.Count == 0)
{
    Console.WriteLine("Brak wygenerowanych wielomianów.");
    return;
}

// wybierz wielomian nieredukowalny jako wielomian redukujący (pierwszy z listy)
int irreducible = coefficients.First(x => IrreducibleFinder.IsIrreducible(x, degree));
var polyCalc = new PolynomialsCalculator(irreducible);

// znajdź a != 0,1 oraz odpowiadające b != 0 (odwrotność w GF(2^8))
int a = 0, b = 0;
foreach (var candidate in coefficients)
{
    if (candidate == 0 || candidate == 1) continue;
    int inv = polyCalc.FindReverse(candidate, coefficients);
    if (inv != 0) { a = candidate; b = inv; break; }
}

if (a == 0 || b == 0)
{
    Console.WriteLine("Nie znaleziono dopasowanych wartości a i b.");
    return;
}

// zaszyfruj i zapisz wynik
var crypter = new Crypter();
var encrypted = crypter.Cipher(content, a, b, polyCalc);
string outPath = filePath + ".enc";
File.WriteAllBytes(outPath, encrypted);

Console.WriteLine($"Plik zaszyfrowany: {outPath} (p={Convert.ToString(irreducible, 2)}, a={a}, b={b})");

var decrypted = crypter.Decipher(encrypted, a, b, polyCalc, coefficients);
string decPath = "/Users/danielkocot/Downloads/dec_" + Path.GetFileName(filePath);
File.WriteAllBytes(decPath, decrypted);