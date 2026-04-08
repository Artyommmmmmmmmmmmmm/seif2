using System.Security.Cryptography;
using System.Text;
private const string fileName = "secret.dat";
// Заглушка для красивого вывода
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("=== Менеджер секретов v1.0 (.NET 8) ===");

const string fileName = "secret.dat";
// В реальном приложении ключ должен генерироваться и храниться в хранилище ОС, 
// для учебной программы сгенерируем его на основе простой соли.
// ВНИМАНИЕ: Не используйте этот способ в продакшене без понимания DPAPI или KeyVault.
byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes("MySecretLearningKey2024!"));

using(var crypto = new AesCryptoService(key));

while (true)
{
    Console.WriteLine("\nВыберите действие:");
    Console.WriteLine("1. Сохранить секретную фразу");
    Console.WriteLine("2. Прочитать секретную фразу");
    Console.WriteLine("3. Выход");
    Console.Write("> ");
    
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await SaveSecretAsync();
            break;
        case "2":
            await ReadSecretAsync();
            break;
        case "3":
            Console.WriteLine("До свидания!");
            return;
        case "4":
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            return;
        default:
            Console.WriteLine("Неверный ввод.");
            break;
    }
}

async Task SaveSecretAsync()
{
    Console.Write("Введите текст для шифрования: ");
    var plainText = Console.ReadLine();
    Console.Write("Введите пароль для шифрования: ");
    string password = Console.ReadLine();
    
    if (string.IsNullOrEmpty(plainText))
    {
        Console.WriteLine("Пустая строка не допускается.");
        return;
    }

    try
    {
        // Шифруем
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        using(var deriva = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA256));
        byte[] key = deriva.GetBytes(32);
        using(var cryptolocal = new AesCryptoService(key));
        byte[] encryptedData = cryptolocal.Encrypt(plainText);
        
        // Сохраняем в файл асинхронно
        byte[] totaldata = salt.Length + plainText.Length;
        Buffer.BlockCopy(salt, 0, totaldata, 0, salt.Length);
        Buffer.BlockCopy(plainText, 0, totaldata, salt.Length, plainText.Length);
        await File.WriteAllBytesAsync(fileName, totaldata);
        Console.WriteLine($"✅ Данные успешно зашифрованы и сохранены в {fileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при сохранении: {ex.Message}");
    }
}

async Task ReadSecretAsync()
{
    if (!File.Exists(fileName))
    {
        Console.WriteLine("⚠️ Файл с секретом не найден. Сначала сохраните данные.");
        return;
    }

    try
    {
        Console.Write("Введите пароль для дешифрования: ");
        string password = password = Console.ReadLine();

        using(byte key = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA256));
        byte[] fileData = await File.ReadAllBytesAsync(fileName);
        byte[] encData = new byte[fileData.Length - 16];
        byte[] salt = new byte[16];
        Buffer.BlockCopy(salt, 0, fileData, 0, 16);
        Buffer.BlockCopy(encData, 0, fileData, 0, 16);
        using(var cryptolocal = new AesCryptoService(key));
        
        string decryptedText = cryptolocal.Decrypt(encryptedData);
        Console.WriteLine($"🔓 Расшифрованный текст: \"{decryptedText}\"");
    }
    catch (CryptographicException)
    {
        Console.WriteLine("❌ Ошибка расшифровки: ключ не подходит или данные повреждены.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка чтения: {ex.Message}");
    }
}

// Вспомогательный класс для шифрования (инкапсулирован в том же файле)
public class AesCryptoService : IDisposable
{
    private readonly Aes _aes;
    private bool _disposed;

    public AesCryptoService(byte[] key)
    {
        _aes = Aes.Create();
        _aes.Key = key;
        _aes.GenerateIV(); // Генерируем случайный вектор инициализации для каждого нового шифрования
    }

    public byte[] Encrypt(string plainText)
    {
        // Генерируем новый IV при каждой операции шифрования для безопасности
        _aes.GenerateIV();
        
        using var encryptor = _aes.CreateEncryptor();
        using var ms = new MemoryStream();
        
        // Сначала записываем IV в поток, чтобы потом использовать при расшифровке
        ms.Write(_aes.IV, 0, _aes.IV.Length);
        
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        
        return ms.ToArray();
    }

    public string Decrypt(byte[] cipherData)
    {
        using var ms = new MemoryStream(cipherData);
        
        // Читаем IV из начала потока
        byte[] iv = new byte[_aes.BlockSize / 8];
        int bytesRead = ms.Read(iv, 0, iv.Length);
        if (bytesRead != iv.Length)
            throw new CryptographicException("Недостаточно данных для IV.");

        _aes.IV = iv;
        
        using var decryptor = _aes.CreateDecryptor();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return sr.ReadToEnd();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _aes?.Dispose();
            _disposed = true;
        }
    }
}
