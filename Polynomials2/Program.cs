// See https://aka.ms/new-console-template for more information

using System.Text;

var L = int.Parse(Console.ReadLine());
var dynamicPolynomials = new HashSet<Polynomial>[L + 1];
dynamicPolynomials[0] = Polynomial.BasePolynomials.ToHashSet();

for (var op1 = 0; op1 <= L; op1++)
    for (var op2 = 0; op2 <= op1 && op1 + op2 + 1 <= L; op2++)
    {
        var ind = op1 + op2 + 1;
        dynamicPolynomials[ind] ??= new();
        foreach (var a in dynamicPolynomials[op1])
            foreach (var b in dynamicPolynomials[op2])
            {
                dynamicPolynomials[ind].Add(a.Sum(b));
                dynamicPolynomials[ind].Add(a.Product(b));
                dynamicPolynomials[ind].Add(a.Subtract(b));
                dynamicPolynomials[ind].Add(b.Subtract(a));
            }
    }

foreach (var item in dynamicPolynomials[L])
    Console.WriteLine(item);

Console.WriteLine(dynamicPolynomials[L].Count);

class Polynomial
{
    private static readonly Polynomial[] basePolynomials = new Polynomial[]
    {
        new(new[] { 0 }, new("0")),
        new(new[] { 1 }, new("1")),
        new(new[] { 2 }, new("2")),
        new(new[] { 1, 0 }, new("x")),
    };

    public static Polynomial[] BasePolynomials => basePolynomials;
    public int[] Coefficients => coefficients;

    private readonly bool IsNeedBrackets;
    private readonly int[] coefficients;
    private readonly StringBuilder expression;
    private readonly int hashCode;

    public Polynomial(int[] coefficients, StringBuilder expression, bool isNeedBrackets = false)
    {
        coefficients = coefficients.SkipWhile(t => t == 0).ToArray();
        this.coefficients = coefficients.Length > 0 ? coefficients : new[] { 0 };
        this.expression = expression;
        hashCode = FNV.CreateHash(coefficients);
        IsNeedBrackets = isNeedBrackets;
    }

    private StringBuilder CreateExpression(
        StringBuilder firstExpression,
        StringBuilder secondExpression,
        char operation,
        bool firstBrackets,
        bool secondBrackets)
    {
        var newExpressionLength = firstExpression.Length + secondExpression.Length + 3 + (firstBrackets ? 2 : 0) + (secondBrackets ? 2 : 0);
        var newExpression = new StringBuilder(newExpressionLength);

        if (firstBrackets) newExpression.Append('(');
        newExpression.Append(firstExpression);
        if (firstBrackets) newExpression.Append(')');

        newExpression.Append($" {operation} ");

        if (secondBrackets) newExpression.Append('(');
        newExpression.Append(secondExpression);
        if (secondBrackets) newExpression.Append(')');

        return newExpression;
    }

    private Polynomial SelectPolynomial(Polynomial other, Func<int, int, int> selector, StringBuilder expression)
    {
        var newCoefficients = new int[Math.Max(coefficients.Length, other.coefficients.Length)];
        coefficients.CopyTo(newCoefficients, newCoefficients.Length - coefficients.Length);

        for (var i = 1; i <= other.coefficients.Length; i++)
            newCoefficients[^i] = selector(newCoefficients[^i], other.coefficients[^i]);

        return new(newCoefficients, expression, true);
    }

    public Polynomial Sum(Polynomial other) => 
        SelectPolynomial(other, (a, b) => a + b, CreateExpression(expression, other.expression, '+', false, false));

    public Polynomial Subtract(Polynomial other) =>
        SelectPolynomial(other, (a, b) => a - b, CreateExpression(expression, other.expression, '-', false, other.IsNeedBrackets));

    public Polynomial Product(Polynomial other)
    {
        var newCoefficients = new int[coefficients.Length + other.coefficients.Length - 1];

        for (var i = 1; i <= coefficients.Length; i++)
            for (var j = 1; j <= other.coefficients.Length; j++)
                newCoefficients[^(i + j - 1)] += coefficients[^i] * other.coefficients[^j];

        return new(
            newCoefficients,
            CreateExpression(expression, other.expression, '*', IsNeedBrackets, other.IsNeedBrackets));
    }

    public override string ToString() => expression.ToString();

    public override bool Equals(object? obj)
    {
        if (obj is Polynomial polynomial)
        {
            if (coefficients.Length != polynomial.coefficients.Length) return false;
            for (var i = 0; i < coefficients.Length; i++)
                if (coefficients[i] != polynomial.coefficients[i]) return false;
            return true;
        }
        else throw new ArgumentException("Argument isn't Polynomial");
    }

    public override int GetHashCode() => hashCode;
}

public static class FNV
{
    public static readonly int OffsetBasis = unchecked((int)2166136261);
    public static readonly int Prime = 16777619;

    public static int CreateHash(int[] data)
    {
        unchecked
        {
            var hash = OffsetBasis;
            foreach (var item in data)
            {
                hash ^= item.GetHashCode();
                hash *= Prime;
            }
            return hash;
        }
    }
}