using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using CCW.Tools;
using System.Text.RegularExpressions;


//v2 - more robust, able to eat any json compliant string
namespace CCW.JsonTools.V2
{
	public interface IEntity
	{
		string? Key { get; set; }
		Node? Parent { get; set; }
	}
	public class Node : IEntity
	{
		public string? Key { get; set; }
		public Node? Parent { get; set; }
		public List<IEntity> Children { get; set; }
		public Node() => Children = new();
		public Node(string key) : this() => this.Key = key;

		public IEntity AddChild(IEntity child)
		{
			this.Children.Add(child);
			child.Parent = this;
			return child;
		}
		public void RemoveChild(IEntity child) => this.Children.Remove(child);
	}
	public class Element : IEntity
	{
		public Node? Parent { get; set; }
		public string? Key { get; set; }
		public dynamic Value { get; set; }
		
		public Element(string key, dynamic value)
		{
			this.Key = key;
			this.Value = value;
		}
	}

	public static class Parser
	{
		public static Node Parse(string input)
		{
			Node root = new();
			input = input.Replace(@"\r", String.Empty).Replace(@"\n", String.Empty);
			input = Regex.Replace(input, @"\s+", String.Empty);
			MiniStack<IEntity> stack = new();
			stack.Push(root);
			
			string matchString = @"(?i)(("".+?""\:((null|true|false)|(\{|\[)|"".*?""|[0-9]{0,10})|\}|\]|\,)|\{)";
			string subMatch = @"("".+?""):";

			foreach (Match match in Regex.Matches(input, matchString))
			{
				dynamic curObj = stack.GetCurrent();
				if (match.Value.Equals("]") || match.Value.Equals("}") || match.Value.Equals(","))
				{
					stack.Pop();
					continue;
				}
				string[] x = Regex.Split(match.Value, subMatch).Where(s => s != String.Empty).ToArray();
				if (x.Length > 1)
				{
					if (x[1].Equals("{") || x[1].Equals("[")) stack.Push(curObj.AddChild(new Node(RemoveQuotes(x[0]))));
					else stack.Push(curObj.AddChild(new Element(RemoveQuotes(x[0]), RemoveQuotes(x[1]))));
				}
				else {
					stack.Push(curObj.AddChild(new Node()));
				}
			}
			
			string RemoveQuotes(string x)
			{
				if (x.Contains('"')) x = x.Remove(x.Length - 1, 1).Remove(0, 1);
				return x;
			}
			return root;
		}

		public static string PrintTree(Node root) => rPrint(root);

		private static readonly char[] jsonChars = { '{', '}', '"', ':', ',', '[', ']' };
		private static int loopCount = 0;

		private static bool IsSpecial(char c) => jsonChars.Contains(c);
		private static string rPrint(Node e)
		{
			loopCount++;
			const string delim = " : ";
			StringBuilder sb = new();
			sb.AppendLine(indent(loopCount) + e.Key);
			foreach (IEntity f in e.Children)
			{
				switch (f)
				{
					case Element el:
						sb.AppendLine($"{indent(loopCount)}{el.Key}{delim}{el.Value}");
						break;
					case Node no:
						sb.Append(rPrint(no));
						break;
				}
			}
			return sb.ToString();
		}
		private static string indent(int count)
		{
			StringBuilder sb = new();
			for (int i = 0; i < count; i++) { sb.Append("\t"); }
			return sb.ToString();
		}

	}
}
