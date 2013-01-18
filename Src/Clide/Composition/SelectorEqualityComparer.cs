﻿#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Composition
{
    using System;
    using System.Collections.Generic;

    /// <summary>
	/// Allows comparisons (and Distinct in linq) by using 
	/// a simple selector function that projects a value 
	/// from a source, to use for comparison.
	/// </summary>
	internal class SelectorEqualityComparer<TSource, TResult> : IEqualityComparer<TSource>
	{
		Func<TSource, TResult> selector;

		public SelectorEqualityComparer(Func<TSource, TResult> selector)
		{
			Guard.NotNull(() => selector, selector);

			this.selector = selector;
		}

		public bool Equals(TSource x, TSource y)
		{
			if (Object.Equals(null, x) || Object.Equals(null, y))
				return false;

			return Object.Equals(selector(x), selector(y));
		}

		public int GetHashCode(TSource obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			return selector(obj).GetHashCode();
		}
	}
}
