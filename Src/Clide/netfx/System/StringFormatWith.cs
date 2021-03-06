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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web;
using System.IO;

/// <summary>
/// Requires a reference to System.Web.
/// </summary>
internal static class StringFormatWithExtension
{
	/// <summary>
	/// Formats the string with the given source object. 
	/// Expression like {Id} are replaced with the corresponding 
	/// property value in the <paramref name="source"/>. Supports 
	/// all <see cref="DataBinder.Eval(object, string)"/> expressions formats 
	/// for property access.
	/// </summary>
	/// <nuget id="netfx-System.StringFormatWith" />
	/// <param name="format" this="true">The string to format</param>
	/// <param name="source">The source object to apply to format</param>
	public static string FormatWith(this string format, object source)
	{
		if (format == null)
			throw new ArgumentNullException("format");

		var result = new StringBuilder(format.Length * 2);

		using (var reader = new StringReader(format))
		{
			var expression = new StringBuilder();
			var @char = -1;

			var state = State.OutsideExpression;
			do
			{
				switch (state)
				{
					case State.OutsideExpression:
						@char = reader.Read();
						switch (@char)
						{
							case -1:
								state = State.End;
								break;
							case '{':
								state = State.OnOpenBracket;
								break;
							case '}':
								state = State.OnCloseBracket;
								break;
							default:
								result.Append((char)@char);
								break;
						}
						break;
					case State.OnOpenBracket:
						@char = reader.Read();
						switch (@char)
						{
							case -1:
								throw new FormatException();
							case '{':
								result.Append('{');
								state = State.OutsideExpression;
								break;
							default:
								expression.Append((char)@char);
								state = State.InsideExpression;
								break;
						}
						break;
					case State.InsideExpression:
						@char = reader.Read();
						switch (@char)
						{
							case -1:
								throw new FormatException();
							case '}':
								result.Append(OutExpression(source, expression.ToString()));
								expression.Length = 0;
								state = State.OutsideExpression;
								break;
							default:
								expression.Append((char)@char);
								break;
						}
						break;
					case State.OnCloseBracket:
						@char = reader.Read();
						switch (@char)
						{
							case '}':
								result.Append('}');
								state = State.OutsideExpression;
								break;
							default:
								throw new FormatException();
						}
						break;
					default:
						throw new InvalidOperationException("Invalid state.");
				}
			} while (state != State.End);
		}

		return result.ToString();
	}

	private static string OutExpression(object source, string expression)
	{
		var format = "";
		var colonIndex = expression.IndexOf(':');

		if (colonIndex > 0)
		{
			format = expression.Substring(colonIndex + 1);
			expression = expression.Substring(0, colonIndex);
		}

		try
		{
			if (string.IsNullOrEmpty(format))
				return (DataBinder.Eval(source, expression) ?? "").ToString();
			else
				return DataBinder.Eval(source, expression, "{0:" + format + "}") ?? "";
		}
		catch (HttpException)
		{
			throw new FormatException("Failed to format '" + expression + "'.");
		}
	}

	private enum State
	{
		OutsideExpression,
		OnOpenBracket,
		InsideExpression,
		OnCloseBracket,
		End
	}
}