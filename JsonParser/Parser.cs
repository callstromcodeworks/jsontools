using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;


//v1 - quick and dirty
namespace CCW.JsonTools.V1
{
	public static class Parser
	{

		public static JsonEntity Parse(string data)
		{
			bool first = true, inQuotes = false, inArray = false, inValue = false;
			StringBuilder sb = new();
			JsonEntity root = new();
			MiniStack stack = new();
			stack.Push(new());
			root.children.Add(stack.GetCurrent());

			foreach (char x in data)
			{
				switch (x)
				{
					case '{':
						if (first)
						{
							first = false;
							continue;
						}
						stack.GetCurrent().children.Add(new());
						stack.Push(stack.GetCurrent().children[^1]);
						inValue = false;
						break;
					case '}':
						stack.Pop();
						break;
					case '"':
						if (inArray) goto default;
						if (inQuotes)
						{
							if (inValue) stack.GetCurrent().value = sb.ToString();
							else stack.GetCurrent().key = sb.ToString();
							sb.Clear();
						}
						inQuotes = !inQuotes;
						break;
					case ':':
						if (inQuotes || inArray) goto default;
						inValue = true;
						break;
					case ',':
						if (!inQuotes && !inArray)
						{
							inValue = false;
							stack.Pop();
							stack.GetCurrent().children.Add(new());
							stack.Push(stack.GetCurrent().children[^1]);
							break;
						}
						goto default;
					case '[':
						if (inValue && !inQuotes)
						{
							inArray = true;
						}
						sb.Append(x);
						break;
					case ']':
						inArray = false;
						inValue = false;
						sb.Append(x);
						stack.GetCurrent().value = sb.ToString();
						sb.Clear();
						break;
					default:
						if (inQuotes || inArray) sb.Append(x);
						break;

				}
			}
			return root;
		}
	}

	public class JsonEntity
	{
		public List<Element> children = new();

		internal const string delim = " : ";

		public string GetContents()
		{
			StringBuilder sb = new();
			foreach (Element e in children)
			{
				sb.AppendLine(GetContents(e, 0));
			}
			return sb.ToString();
		}

		internal string GetContents(Element e, int depth)
		{
			StringBuilder sb = new();
			if (depth > 0)
			{
				for (int i = 0; i <= depth; i++)
				{
					sb.Append("\t\t\t");
				}
			}
			sb.AppendLine(e.key + delim + e.value);
			if (e.children.Count > 0)
			{
				foreach (Element f in e.children)
				{
					sb.AppendLine(GetContents(f, depth + 1));
				}
			}
			return sb.ToString();
		}

		public class Element : IEnumerator<Element>, IEnumerable<Element>
		{
			public string key { get; set; }
			public string value { get; set; }
			public List<Element> children = new();

			private bool disposedValue;
			int position = -1;

			public Element Current()
			{
				return children[position];
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current();
				}
			}

			Element IEnumerator<Element>.Current => throw new NotImplementedException();

			public bool MoveNext()
			{
				position++;
				return position >= children.Count;
			}

			public void Reset()
			{
				position = -1;
			}
			
			protected void DisposeMe()
			{
				foreach (Element e in children)
				{
					DisposeChildren(e);
				}
			}

			protected void DisposeChildren(Element e)
			{
				if (e.children.Count > 0)
				{
					foreach (Element f in e.children)

					{
						DisposeChildren(f);
					}
					e.children.Clear();
				}
				
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						// TODO: dispose managed state (managed objects)
						DisposeMe();
					}

					// TODO: free unmanaged resources (unmanaged objects) and override finalizer
					// TODO: set large fields to null
					disposedValue = true;
				}
			}

			// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
			// ~Element()
			// {
			//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			//     Dispose(disposing: false);
			// }

			public void Dispose()
			{
				// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			public IEnumerator<Element> GetEnumerator()
			{
				return ((IEnumerable<Element>)children).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)children).GetEnumerator();
			}
		}
	}

	public class MiniStack
	{
		public List<JsonEntity.Element> stack = new();

		public void Push(JsonEntity.Element e)
		{
			stack.Add(e);
		}
		public void Pop()
		{
			stack.RemoveAt(stack.Count - 1);
		}
		public JsonEntity.Element GetCurrent()
		{
			return stack[^1];
		}
	}

}
