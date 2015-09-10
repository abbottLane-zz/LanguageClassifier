using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Linq;

namespace Ling473Lab5
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			//---=== Read in each language model to memory ====--
			List<LanguageModel> lms = new List<LanguageModel>();

			string[] docs = Directory.GetFiles("../../resources/LanguageModels/", "*.unigram-lm");
			foreach (string doc in docs) // ---== foreach language model...
			{
				System.IO.StreamReader file = new System.IO.StreamReader (doc, System.Text.Encoding.GetEncoding("iso-8859-15"));
				LanguageModel ln = new LanguageModel (file, getLanguageName(doc)); //---== ...create a language model object
				lms.Add (ln);
			}

			// ---=== Classify input against language model info
			System.IO.StreamReader input = new System.IO.StreamReader("../../resources/train.txt", System.Text.Encoding.GetEncoding("iso-8859-15"));
		
			string line;
			while ((line = input.ReadLine ()) != null) 
			{
				Console.WriteLine (line);
				line = StripPunctuation (line);
				Tuple<string, double> winningest = new Tuple<string, double> ("", Double.NegativeInfinity);
				foreach (LanguageModel l in lms) 
				{
					string[] words = Regex.Split (line, @"\s");
					double runningProbability = 0;
					foreach (string word in words) {
						runningProbability += (Math.Log10((l.getNormalizedOccurencesOfWord (word)) / 10000000.0));
					}
					if (runningProbability > winningest.Item2 ) {
						winningest = new Tuple<string, double> (l.getLanguage (), runningProbability);
					}
					Console.WriteLine (l.getLanguage() + "\t" + runningProbability);
				}
				Console.WriteLine ("result "+ winningest.Item1);
			}

		}
		public static string getLanguageName(string dir_in)
		{
			string[] splitDir = dir_in.Split ('/');
			return splitDir [splitDir.Length - 1].Substring (0, 3);
		}

		public static string StripPunctuation(string s)
		{
			var sb = new StringBuilder ();
			foreach (char c in s) {
				if (!char.IsPunctuation (c))
					sb.Append (c);
			}
			return sb.ToString ();
		}
	}

	class LanguageModel
	{
		private string lang;
		private int total_occurences;
		private Dictionary<string, int> model;

		public LanguageModel(StreamReader file_in, string lang_in)
		{
			lang = lang_in;
			model = new Dictionary<string, int> ();
			//smoothingVal = 0.0001; //Virtually zero, because giving any weight to 
								//unfound characters gives disproportionate weight to sparser language models  

			//populate dictionary of language model terms and counts
			string line;
			while ((line = file_in.ReadLine ()) != null) {

				//Split input into string keys and occurence count values
				string[] valueAndKey = line.Split ('\t');
				int value = Int32.Parse (valueAndKey [1]);
				total_occurences += value;
				model.Add (valueAndKey [0], value );

			}

		}
		public int getTotalOccurancesInAModel()
		{
			return total_occurences;
		}
		public double getNormalizedOccurencesOfWord(string str)
		{
			double multiplyer = 10000000.0/total_occurences;

			if (model.ContainsKey (str)) {
				return (model [str]*multiplyer);
			} else {
				return multiplyer;
			}
		}
		public string getLanguage()
		{
			return this.lang;
		}

	}
}

