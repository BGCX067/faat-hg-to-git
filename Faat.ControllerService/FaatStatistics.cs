using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System;

using MyUtils;

namespace Faat.ControllerService
{
	public class FaatStatistics : IFaatStatisticsWriter
	{
		class Entry
		{
			readonly Func<Entry, string> _factory;
			readonly ParameterKind _kind;

			public ParameterKind Kind
			{
				get { return _kind; }
			}

			public Entry(ParameterKind kind, Func<Entry, string> factory)
			{
				if (factory == null)
				{
					throw new ArgumentNullException("factory");
				}
				_factory = factory;
				_kind = kind;
			}

			public string Value
			{
				get { return _factory(this); }
			}
		}

		readonly Dictionary<string, Entry> _statistics = new Dictionary<string, Entry>();
		readonly Dictionary<string, decimal> _statisticsNumeric = new Dictionary<string, decimal>();

		public IEnumerable<string> ParameterNames
		{
			get 
			{
				lock (_statistics)
				{
					return _statistics.Keys.ToArray();
				}
			}
		}

		public string GetParameterValue(string parameterName)
		{
			return _statistics[parameterName].Value;
		}

		public void NumericSum(string parameterName, decimal value, ParameterKind kind = ParameterKind.Numeric)
		{
			lock (_statistics)
			{
				Entry entry;
				if (!_statistics.TryGetValue(parameterName, out entry))
				{
					_statistics.Add(parameterName, new Entry(kind, e => Display(e, _statisticsNumeric[parameterName])));
					_statisticsNumeric.Add(parameterName, value);
				}
				else
				{
					_statisticsNumeric[parameterName] += value;
				}
			}
		}

		public void NumericMax(string parameterName, decimal value, ParameterKind kind = ParameterKind.Numeric)
		{
			lock (_statistics)
			{
				Entry entry;
				if (!_statistics.TryGetValue(parameterName, out entry))
				{
					_statistics.Add(parameterName, new Entry(kind, e => Display(e, _statisticsNumeric[parameterName])));
					_statisticsNumeric[parameterName] = 0;
				}
				_statisticsNumeric[parameterName] = Math.Max(_statisticsNumeric[parameterName], value);
			}
		}

		static string Display(Entry entry, decimal value)
		{
			switch (entry.Kind)
			{
				case ParameterKind.Numeric:
					return value.ToString(CultureInfo.InvariantCulture);
				case ParameterKind.InfoSize:
					return ((InfoSize)value).ToString(null, CultureInfo.InvariantCulture);
				case ParameterKind.TimeSpan:
					return new TimeSpan(checked((long)value)).ToSpanString();
				case ParameterKind.Custom: // only for custom funcs
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void NumericAverrage(string parameterName, decimal value, ParameterKind kind = ParameterKind.Numeric)
		{
			lock (_statistics)
			{
				Entry entry;
				if (!_statistics.TryGetValue(parameterName, out entry))
				{
					_statistics.Add(parameterName, new Entry(kind, e =>
					                                               Display(e, (_statisticsNumeric[parameterName + "__sum"] / _statisticsNumeric[parameterName + "__count"]))));
					_statisticsNumeric[parameterName + "__sum"] = value;
					_statisticsNumeric[parameterName + "__count"] = 1;
				}
				else
				{
					_statisticsNumeric[parameterName + "__sum"] += value;
					_statisticsNumeric[parameterName + "__count"] += 1;
				}
			}
		}

		public void RegisterDynamicParameter(string parameterName, Func<IFaatStatistics, string> factory)
		{
			_statistics.Add(parameterName, new Entry(ParameterKind.Custom, _ => factory(this)));
		}
	}
}