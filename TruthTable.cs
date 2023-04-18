namespace TruthTable
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;

	public static class TruthTable
	{
		static readonly Form f = new Form();
		static readonly DataGridView dgv = new DataGridView();

		//options
		static readonly char openBracket = '(';
		static readonly char closeBracket = ')';
		static readonly char not = '!';
		static readonly char and = '&';
		static readonly char or = '|';
		static readonly string expression = "a & b & c & d & e"; //A & !C & (D | !B & (B & !A) | A) & (C | !C)
		static readonly HashSet<string> variables = Variables(expression);
		static string expressionCode = GetExpressionCode(expression);

		[STAThread]
		static void Main(string[] args)
		{
			dgv.Dock = DockStyle.Fill;
			f.Controls.Add(dgv);

			PrintTable(expression);
			f.ShowDialog();
			Console.Read();
		}

		static void PrintTable(string expression)
		{
			List<List<string>> matrix = GetMatrix(expression);
			List<string> head = new List<string>();
			foreach (var variable in variables)
			{
				head.Add($"{variable,3}");
				dgv.Columns.Add(variable, variable);
			};
			matrix = matrix.Prepend(head).ToList();

			matrix.ForEach(row =>
			{
				dgv.Rows.Add(row.ToArray());
			});
			variables.ToList().ForEach(variable => Console.WriteLine(variable));
		}

		static List<List<string>> GetMatrix(string expression)
		{
			List<List<string>> matrix = new List<List<string>>();

			int variablesCount = Variables(expression).Count();
			int matrixHeight = (int)Math.Pow(2, variablesCount);

			matrix.Add(new List<string>());

			for (int i = 0; i < matrixHeight; i++)
			{
				matrix.Add(new List<string>());
				for (int j = 0; j < variablesCount; j++)
				{
					matrix[i].Add(string.Empty);
				}
			}

			for (int letterPosition = 0; letterPosition < variablesCount; letterPosition++)
			{
				int repeatAfter = (int)(matrixHeight / Math.Pow(2, letterPosition + 1));
				string answer = "0";
				for (int i = 0; i < matrixHeight; i++)
				{
					if (i % repeatAfter == 0) answer = answer.Equals("1") ? "0" : "1"; //Перевернуть 0 и 1
					matrix[i][letterPosition] = answer;
				}
			}

			Calculate(matrix, expressionCode);
			return matrix;
		}

		static void Calculate(List<List<string>> matrix, string expressionCodePart)
		{
			Console.WriteLine($"Calculate enter {expressionCodePart}");

			while (expressionCode.Contains($"{openBracket}"))
			{
				Console.WriteLine($"Bracket enter   {expressionCodePart} {expressionCode}");
				string nextBrackets = GetNextBrackets(expressionCodePart);

				string newCodePart = variables.Contains(nextBrackets) ? $"{{{variables.ToList().IndexOf(nextBrackets)}}}" : $"{{{variables.Count()}}}";

				CalculateBrackets(matrix, nextBrackets);
				expressionCodePart = expressionCode;
				Console.WriteLine($"Bracket exit {newCodePart} {expressionCode}");
			}
			// After this line expressionCodePart has no more Brackets

			while (expressionCodePart.Contains($"{not}"))
			{
				Console.WriteLine($"Not enter {expressionCodePart}");

				string[] expressionCodeArray = GetExpressionArray(expressionCodePart);
				int i = expressionCodeArray.ToList().IndexOf($"{not}");

				string next = expressionCodeArray[i + 1];

				string oldNotCodePart = not + next;
				string newNotVariable = not + variables.ToList()[GetIndex(next)];
				string newCodePart = variables.Contains($"{newNotVariable}") ? $"{{{variables.ToList().IndexOf(newNotVariable)}}}" : $"{{{variables.Count()}}}";
				
				variables.Add(newNotVariable);
				for (int x = 0; x < matrix.Count - 1; x++)
				{
					matrix[x].Add(ToString(!GetValue(matrix[x], next)));
				}

				expressionCodePart = expressionCodePart.Replace(oldNotCodePart, newCodePart);
				Console.WriteLine($"Not exit  {oldNotCodePart} {newCodePart}");
			}
			// After this line expressionCodePart has no more Negation

			while (expressionCodePart.Contains($"{and}"))
			{
				Console.WriteLine($"And enter {expressionCodePart}");
				string[] expressionCodeArray = GetExpressionArray(expressionCodePart);
				int i = expressionCodeArray.ToList().IndexOf($"{and}");
				string prev = expressionCodeArray[i - 1];
				string next = expressionCodeArray[i + 1];
				string oldAndCodePart = prev + and + next;
				string newAndVariable = variables.ToList()[GetIndex(prev)] + and + variables.ToList()[GetIndex(next)];
				string newCodePart = variables.Contains(newAndVariable) ? $"{{{variables.ToList().IndexOf(newAndVariable)}}}" : $"{{{variables.Count()}}}";

				variables.Add(newAndVariable);
				for (int x = 0; x < matrix.Count - 1; x++)
				{
					matrix[x].Add(ToString(GetValue(matrix[x], prev) && GetValue(matrix[x], next)));
				}

				expressionCodePart = expressionCodePart.Replace(oldAndCodePart, newCodePart);
				Console.WriteLine($"And exit  {expressionCodePart}");
			}
			// After this line expressionCodePart has no more Conjunctions

			while (expressionCodePart.Contains(or))
			{
				Console.WriteLine($"Or enter  {expressionCodePart}");
				string[] expressionCodeArray = GetExpressionArray(expressionCodePart);
				int i = expressionCodeArray.ToList().IndexOf($"{or}");
				string prev = expressionCodeArray[i - 1];
				string next = expressionCodeArray[i + 1];
				string oldOrCodePart = prev + or + next;
				string newOrVariable = variables.ToList()[GetIndex(prev)] + or + variables.ToList()[GetIndex(next)];
				string newCodePart = variables.Contains(newOrVariable) ? $"{{{variables.ToList().IndexOf(newOrVariable)}}}" : $"{{{variables.Count()}}}";

				variables.Add(newOrVariable);
				for (int x = 0; x < matrix.Count - 1; x++)
				{
					matrix[x].Add(ToString(GetValue(matrix[x], prev) || GetValue(matrix[x], next)));
				}

				expressionCodePart = expressionCodePart.Replace(oldOrCodePart, newCodePart);
				Console.WriteLine($"Or exit   {expressionCodePart}");
			}
			// After this line expressionCodePart has no more Disjunction

			// After this line expressionCodePart has no more operations
			// (result of brackets calculation)
			// {4}

			Console.WriteLine($"Calculate exit {expressionCodePart} {expressionCode}");
		}

		static void CalculateBrackets(List<List<string>> matrix, string bracketsCodePart)
		{
			Console.WriteLine($"CalculateBrackets enter {bracketsCodePart}");
			// After this line expressionCodePart has no more Brackets
			string oldCodePart = $"({bracketsCodePart})";

			while (bracketsCodePart.Contains($"{not}"))
			{
				Console.WriteLine($"Not enter {bracketsCodePart}");

				string[] expressionCodeArray = GetExpressionArray(bracketsCodePart);
				int i = expressionCodeArray.ToList().IndexOf($"{not}");

				string next = expressionCodeArray[i + 1];

				string oldNotCodePart = not + next;
				string newNotVariable = not + variables.ToList()[GetIndex(next)];
				string newCodePart = variables.Contains($"{newNotVariable}") ? $"{{{variables.ToList().IndexOf(newNotVariable)}}}" : $"{{{variables.Count()}}}";

				variables.Add(newNotVariable);
				for (int x = 0; x < matrix.Count - 1; x++)
				{
					matrix[x].Add(ToString(!GetValue(matrix[x], next)));
				}

				bracketsCodePart = bracketsCodePart.Replace(oldNotCodePart, newCodePart);
				Console.WriteLine($"Not exit  {oldNotCodePart} {newCodePart}");
			}
			// After this line expressionCodePart has no more Negation

			while (bracketsCodePart.Contains($"{and}"))
			{
				Console.WriteLine($"And enter {bracketsCodePart}");
				string[] expressionCodeArray = GetExpressionArray(bracketsCodePart);
				int i = expressionCodeArray.ToList().IndexOf($"{and}");
				string prev = expressionCodeArray[i - 1];
				string next = expressionCodeArray[i + 1];
				string oldAndCodePart = prev + and + next;
				string newAndVariable = variables.ToList()[GetIndex(prev)] + and + variables.ToList()[GetIndex(next)];
				string newCodePart = variables.Contains(newAndVariable) ? $"{{{variables.ToList().IndexOf(newAndVariable)}}}" : $"{{{variables.Count()}}}";

				variables.Add(newAndVariable);
				for (int x = 0; x < matrix.Count - 1; x++)
				{
					matrix[x].Add(ToString(GetValue(matrix[x], prev) && GetValue(matrix[x], next)));
				}

				bracketsCodePart = bracketsCodePart.Replace(oldAndCodePart, newCodePart);
				Console.WriteLine($"And exit  {bracketsCodePart}");
			}
			// After this line expressionCodePart has no more Conjunctions

			while (bracketsCodePart.Contains(or))
			{
				Console.WriteLine($"Or enter  {bracketsCodePart}");
				string[] expressionCodeArray = GetExpressionArray(bracketsCodePart);
				int i = expressionCodeArray.ToList().IndexOf($"{or}");
				string prev = expressionCodeArray[i - 1];
				string next = expressionCodeArray[i + 1];
				string oldOrCodePart = prev + or + next;
				string newOrVariable = variables.ToList()[GetIndex(prev)] + or + variables.ToList()[GetIndex(next)];
				string newCodePart = variables.Contains(newOrVariable) ? $"{{{variables.ToList().IndexOf(newOrVariable)}}}" : $"{{{variables.Count()}}}";

				variables.Add(newOrVariable);
				for (int x = 0; x < matrix.Count - 1; x++)
				{
					matrix[x].Add(ToString(GetValue(matrix[x], prev) || GetValue(matrix[x], next)));
				}

				bracketsCodePart = bracketsCodePart.Replace(oldOrCodePart, newCodePart);
				Console.WriteLine($"Or exit   {bracketsCodePart}");
			}
			// After this line expressionCodePart has no more Disjunction

			// After this line expressionCodePart has no more operations
			// (result of brackets calculation)
			// {4}
			expressionCode = expressionCode.Replace(oldCodePart, bracketsCodePart);
			Console.WriteLine($"CalculateBrackets exit {bracketsCodePart} {expressionCode}");
		}

		static string GetNextBrackets(string expression)
		{
			if (!expression.Contains(openBracket))
				return expression;

			if (expression.Count(c => c == openBracket) != expression.Count(c => c == closeBracket))
				throw new Exception("Wrong bracket arrangement");

			string result = Regex.Match(expression, @"\((\w+|[^()]*)\)").Groups[1].Value;
			return result;
		}

		static string GetExpressionCode(string expression)
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

		static string[] GetExpressionArray(string expression)
		{
			return Regex.Matches(expression, @"([&|])\1|{\d+}|\S")
				.Cast<Match>()
				.Select(match => match.Value)
				.ToArray();
		}

		static int GetIndex(string expressionPart)
		{
			/// Returns index from expressionCodePart like {0}
			if (expressionPart.Length < 3) throw new Exception($"Wrong expressionPart \"{expressionPart}\" lenght");
			return Convert.ToInt32(expressionPart.Replace("{", "").Replace("}", ""));
		}

		static bool GetValue(List<string> row, string expressionPart)
		{
			return Convert.ToBoolean(Convert.ToInt32(row[GetIndex(expressionPart)]));
		}

		static string ToString(bool x)
		{
			if (x) return "1";
			else return "0";
		}

		static HashSet<string> Variables(string expression)
		{
			List<string> temp = Regex.Matches(expression, $@"\w+")
				.Cast<Match>()
				.Select(m => m.Value)
				.OrderBy(x => x)
				.ToList();

			return new HashSet<string>(temp);
		}
	}
}
