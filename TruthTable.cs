namespace TruthTable
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Text.RegularExpressions;

	public static class TruthTable
	{
		// A & !C & (D | !B & (B | !A) | A) & (C | !C)
		// A * !C * (D + !B * (B + !A) + A) * (C + !C)

		//options
		static readonly char not = '!';
		static readonly char openBracket = '(';
		static readonly char closeBracket = ')';
		static readonly char and = '&';
		static readonly char or = '|';
		static readonly char space = ' ';
		static readonly string[] operators = { $"{and}", $"{or}", $"{not}", $"{openBracket}", $"{closeBracket}", $"{space}" };

		static readonly string expression = "A & !C & (D | !B & (B | !A) | A) & (C | !C)";


		static readonly HashSet<string> variables = Variables(expression);

		static void Main(string[] args)
		{
			PrintTable(expression);
			Console.WriteLine(PrepareString(expression));
			Console.Read();
		}

		static void PrintTable(string expression)
		{
			List<List<string>> matrix = GetMatrix(expression);
			List<string> head = new List<string>();
			foreach (var variable in variables) head.Add($"{variable,5}");
			matrix = matrix.Prepend(head).ToList();

			matrix.ForEach(row =>
			{
				row.ForEach(column => Console.Write($"{column,5}"));
				Console.WriteLine();
			});
		}

		static List<List<string>> GetMatrix(string expression)
		{
			List<List<string>> matrix = new List<List<string>>();

			int lettersCount = Variables(expression).Count();
			int matrixHeight = (int)Math.Pow(2, lettersCount);

			matrix.Add(new List<string>());

			for (int i = 0; i < matrixHeight; i++)
			{
				matrix.Add(new List<string>());
				for (int j = 0; j < lettersCount; j++)
				{
					matrix[i].Add(string.Empty);
				}
			}

			for (int letterPosition = 0; letterPosition < lettersCount; letterPosition++)
			{
				int repeatAfter = (int)(matrixHeight / Math.Pow(2, letterPosition + 1));
				string answer = "0";
				for (int i = 0; i < matrixHeight; i++)
				{
					if (i % repeatAfter == 0) answer = answer == "1" ? "0" : "1"; //Перевернуть 0 и 1
					matrix[i][letterPosition] = answer;
				}
			}

			Calculate(matrix, expression);
			return matrix;
		}

		static List<List<string>> Calculate(List<List<string>> matrix, string expression)
		{
			for (int i = 0; i < expression.Length; i++)
			{
				string expressionCode = PrepareString(expression);
				//somehow process expressionCode
				//write all logic functions

				char chr = expression[i];
				//if (!operators.Contains($"{chr}")) variables.Add($"{chr}");

				if (variables.Contains($"{chr}")) continue;

				if (chr == not) /*matrix.AddColumn(next => !next)*/;
				if (chr == and) /*(prev, next) => prev && next*/;
				if (chr == or) continue;
				if (chr == openBracket) /*Calculate(matrix, FindBrackets(expression))*/;
				if (chr == space) continue;
			}

			//	else if (expression[i] == or) /*(prev, next) => prev || next*/;
			return matrix;
		}

		public static string FindNextBrackets(string expression)
		{
			int startIndex = expression.IndexOf(openBracket);
			int endIndex = 0;
			int brackets = 0;

			for (int i = startIndex; i < expression.Length - 1; i++)
			{
				char c = expression[i];

				if (c == openBracket) brackets++;
				if (c == closeBracket) brackets--;
				if (brackets == 0)
				{
					endIndex = i + 1;
					return expression.Substring(startIndex, endIndex - startIndex);
				}
			}
			throw new Exception("Wrong brackets arrangement");
		}

		static string PrepareString(string expression)
		{
			string expressionCode = expression;
			expressionCode = expressionCode.Replace(" ", "");

			int index = 0;
			foreach (string x in variables)
			{
				expressionCode = expressionCode.Replace(x, $"{{{index}}}");
				index++;
			}

			return expressionCode;
		}

		static HashSet<string> Variables(string expression)
		{
			List<string> temp = Regex.Matches(expression, $@"[^{string.Join(string.Empty, operators)}]+")
				.Cast<Match>()
				.Select(m => m.Value)
				.OrderBy(x => x)
				.ToList();

			return new HashSet<string>(temp);
		}
	}
}
