using System.Linq;
using System.Collections.Generic;
using System;

namespace Faat
{
	public static class Cache
	{
		public static Cache<T> New<T>(T value)
		{
			return new Cache<T>(value);
		}

		public static Cache<T> New<T>(Func<T> factory)
		{
			return new Cache<T>(factory);
		}
	}

	public sealed class Cache<T>
	{
		readonly Func<T> _factory;
		T _value;
		bool _hasValue;

		public bool IsValueCreated
		{
			get { return _hasValue; }
		}

		public Cache(T value)
		{
			_value = value;
			_hasValue = true;
		}

		public Cache(Func<T> factory)
		{
			_factory = factory;
		}

		public T Value
		{
			get
			{
				var val = _value;
				if (!_hasValue)
				{
					var fact = _factory;
					if (fact != null)
					{
						_value = val = fact();
						_hasValue = true;
					}
				}
				return val;
			}
			set
			{
				_hasValue = true;
				_value = value;
			}
		}

		public void Reset()
		{
			_hasValue = false;
			_value = default(T);
		}

		public static implicit operator Cache<T>(T value)
		{
			return new Cache<T>(value);
		}

		public static implicit operator T(Cache<T> cache)
		{
			return cache.Value;
		}
	}
}