using System;
using System.Collections.Generic;

namespace CCW.Tools
{
	public class MiniStack<T>
	{
		public int Count
		{
			get
			{
				return stack.Count;
			}
		}
		public List<T> stack = new();
		public void Push(T o) => stack.Add(o);
		public void Pop() => stack.RemoveAt(stack.Count - 1);
		public object GetCurrent()
		{
			if (IsEmpty()) return null;
			return stack[^1];
		}
		public bool IsEmpty() => stack.Count == 0;
	}
}