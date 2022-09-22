namespace TruthTable
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	static class ExtensionMethods
	{
		delegate bool Function();

		public static List<List<string>> AddColumn(this List<List<string>> matrix, Func<bool, bool, bool> func)
		{

			return matrix;
		}


		public static List<List<string>> AddColumn(this List<List<string>> matrix/*, Func<bool, bool> func*/)
		{

			return matrix;
		}
	}
}
