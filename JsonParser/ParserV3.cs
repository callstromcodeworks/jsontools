using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using CCW.Tools;
using System.Text.RegularExpressions;


//v3 - error handling
namespace CCW.JsonTools.V3
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
			string validJson = @"(?i)(\{("".+?""\:?("".+?""|true|false|null|[0-9]{0,10}|\[(\{?\2+\}?\,?)\]|\{\2+\})\,?)\}\,?)";
			foreach(Match t in Regex.Matches(input, validJson))
			{
				Console.WriteLine($"matched {t.Value}\n\n\n");
			}

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
				else
				{
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

		public static string PrintTree(Node root)
		{
			int loopCount = 0;
			string delim = " : ";

			string Recursive(Node node)
			{
				StringBuilder sb = new();
				loopCount++;
				sb.AppendLine(Indent(loopCount) + node.Key);
				foreach (IEntity f in node.Children)
				{
					switch (f)
					{
						case Element el:
							sb.AppendLine($"{Indent(loopCount)}{el.Key}{delim}{el.Value}");
							break;
						case Node no:
							sb.Append(Recursive(no));
							break;
					}
				}
				return ""; //sb.ToString();
			}

			string Indent(int count)
			{
				StringBuilder sb = new();
				for (int i = 0; i < count; i++) { sb.Append("\t"); }
				return sb.ToString();
			}
			return Recursive(root);
		}
	}
}
