using System;
using System.Reflection;

namespace Chess_Server.Modules
{
	public abstract class Singleton<T> where T : class
	{
		private static readonly Lazy<T> instance = new Lazy<T>(CreateInstance);

		public static T Instance => instance.Value;

		private static T CreateInstance()
		{
			ConstructorInfo? constructor = typeof(T).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, Type.EmptyTypes, null);

			if (constructor == null)
			{
				throw new InvalidOperationException($"{typeof(T)} must define a private constructor with no parameters to be used as a singleton.");
			}

			return (T)constructor.Invoke(null);
		}

		protected Singleton()
		{
			//
		}
	}
}
