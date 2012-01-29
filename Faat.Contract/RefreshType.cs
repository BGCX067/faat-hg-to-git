﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faat.Core
{
	public enum RefreshType
	{
		None,
		Pool, // state of a test changed by pooling for something periodically or manually
		Push, // state of a test changed by listening for some specific event
		PoolPush,
	}


}
