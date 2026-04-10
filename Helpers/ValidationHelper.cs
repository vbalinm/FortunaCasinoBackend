namespace FortunaCasino.Helpers;

public static class ValidationHelper
{
    //Játéktípusonkénti szabályok
    private static readonly Dictionary<string, GameRule> GameRules = new()
    {
        { "Lottery5",     new GameRule(MinNum: 1,  MaxNum: 90, PickCount: 5,  BonusMin: null, BonusMax: null, BonusCount: 0) },
        { "Lottery6",     new GameRule(MinNum: 1,  MaxNum: 45, PickCount: 6,  BonusMin: null, BonusMax: null, BonusCount: 0) },
        { "Scandinavian", new GameRule(MinNum: 1,  MaxNum: 35, PickCount: 7,  BonusMin: null, BonusMax: null, BonusCount: 0) },
        { "Eurojackpot",  new GameRule(MinNum: 1,  MaxNum: 50, PickCount: 5,  BonusMin: 1,    BonusMax: 12,   BonusCount: 2) },
        { "Keno",         new GameRule(MinNum: 1,  MaxNum: 80, PickCount: 10, BonusMin: null, BonusMax: null, BonusCount: 0) },
        { "Joker",        new GameRule(MinNum: 0,  MaxNum: 9,  PickCount: 6,  BonusMin: null, BonusMax: null, BonusCount: 0) },
    };

    //Szelvényszámok validálása gameType alapján
    public static ValidationResult ValidateNumbers(string fieldsNumbers, string gameType)
    {
        if (string.IsNullOrWhiteSpace(fieldsNumbers))
            return ValidationResult.Fail("A számok nem lehetnek üresek");

        if (!GameRules.TryGetValue(gameType, out var rule))
            return ValidationResult.Fail($"Ismeretlen játéktípus: {gameType}");

        //Joker külön szabály: 6 számjegy 0-9 között
        if (gameType == "Joker")
        {
            if (fieldsNumbers.Length != 6)
                return ValidationResult.Fail("Joker: pontosan 6 számjegy szükséges");

            if (!fieldsNumbers.All(char.IsDigit))
                return ValidationResult.Fail("Joker: csak számjegyek engedélyezettek (0-9)");

            return ValidationResult.Ok();
        }

        //Elválasztás '|' jellel 
        var fields = fieldsNumbers.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length == 0)
            return ValidationResult.Fail("Legalább egy mező szükséges");

        foreach (var field in fields)
        {
            var parseResult = ParseAndValidateField(field, rule);
            if (!parseResult.IsValid) return parseResult;
        }

        return ValidationResult.Ok();
    }

    private static ValidationResult ParseAndValidateField(string field, GameRule rule)
    {
        var parts = field.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != rule.PickCount)
            return ValidationResult.Fail($"Pontosan {rule.PickCount} szám szükséges (kapott: {parts.Length})");

        var numbers = new List<int>();
        foreach (var part in parts)
        {
            if (!int.TryParse(part.Trim(), out int num))
                return ValidationResult.Fail($"Érvénytelen szám: '{part}'");

            if (num < rule.MinNum || num > rule.MaxNum)
                return ValidationResult.Fail($"A számnak {rule.MinNum} és {rule.MaxNum} közé kell esnie (kapott: {num})");

            if (numbers.Contains(num))
                return ValidationResult.Fail($"Ismétlődő szám: {num}");

            numbers.Add(num);
        }

        return ValidationResult.Ok();
    }

    //Ár validálás
    public static ValidationResult ValidatePrice(decimal price, decimal expectedPrice)
    {
        if (price <= 0)
            return ValidationResult.Fail("Az ár nem lehet nulla vagy negatív");

        if (price != expectedPrice)
            return ValidationResult.Fail($"Érvénytelen ár: {price} Ft (elvárt: {expectedPrice} Ft)");

        return ValidationResult.Ok();
    }

    //Feltöltési összeg validálás
    public static ValidationResult ValidateTopUpAmount(decimal amount)
    {
        if (amount <= 0)
            return ValidationResult.Fail("Az összegnek pozitívnak kell lennie");

        if (amount > 1_000_000)
            return ValidationResult.Fail("Maximum feltölthető összeg: 1 000 000 Ft");

        if (amount % 1 != 0)
            return ValidationResult.Fail("Csak egész forint összeg adható meg");

        return ValidationResult.Ok();
    }

    //Mennyiség validálás
    public static ValidationResult ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            return ValidationResult.Fail("A mennyiség legalább 1 kell legyen");

        if (quantity > 20)
            return ValidationResult.Fail("Maximum 20 szelvény vásárolható egyszerre");

        return ValidationResult.Ok();
    }
}

//Játékszabály rekord
public record GameRule(int MinNum, int MaxNum, int PickCount, int? BonusMin, int? BonusMax, int BonusCount);

//Validációs eredmény
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    public static ValidationResult Ok() => new() { IsValid = true };
    public static ValidationResult Fail(string msg) => new() { IsValid = false, ErrorMessage = msg };
}