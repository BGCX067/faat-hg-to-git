using System.Linq;
using System.Collections.Generic;
using System;

using MyUtils;

namespace Faat.Agent
{
	public class FaatAgentExecutor// : IDisposable
	{

		public void RunPage(string serverAddress, string pageIdentifier, string resultDataIdentifier)
		{
			using (var dom = new SeparateDomain())
			{
				var exRemote = (Exception)dom.Execute<FaatAgent, object>(x =>
				{
					var ex = x.RunPage(serverAddress, pageIdentifier, resultDataIdentifier);
					return ex;
				});
				if (exRemote != null)
				{
					throw exRemote;
				}
			}
		}

//		protected virtual void Dispose(bool disposing)
//		{
//			if (disposing)
//			{
//				
//			}
//			var d = _dom;
//			if (d != null)
//			{
//				d.Dispose();
//			}
//		}
//
//		public void Dispose()
//		{
//			GC.SuppressFinalize(this);
//			Dispose(true);
//		}
//
//		~FaatAgentExecutor()
//		{
//			Dispose(false);
//		}
	}
}