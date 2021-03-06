﻿#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.ComponentModel;
    using Microsoft.VisualStudio.Shell.Settings;
    using Microsoft.VisualStudio.Settings;
    using System.ComponentModel.DataAnnotations;

    public class SettingsManagerSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenASimpleClass
		{
			private WritableSettingsStore settingsStore;

			[TestInitialize]
			public void Initialize()
			{
				var shellManager = new ShellSettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);
				this.settingsStore = shellManager.GetWritableSettingsStore(SettingsScope.UserSettings);

				var collection = SettingsManager.GetSettingsCollectionName(typeof(Foo));
				if (this.settingsStore.CollectionExists(collection))
					this.settingsStore.DeleteCollection(collection);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenReadingSettingsAndNoExistingOne_ThenReadsDefaultValues()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

				var foo = new Foo();
				manager.Read(foo);

				Assert.Null(foo.StringProperty);
				Assert.Equal("Hello", foo.DefaultValueStringProperty);
				Assert.Equal(0, foo.IntProperty);
				Assert.Equal(5, foo.DefaultValueIntProperty);
				Assert.Equal(25, foo.DefaultValueAsStringIntProperty);
				Assert.Equal(0, (int)foo.EnumProperty);
				Assert.Equal(UriFormat.Unescaped, foo.DefaultValueEnumProperty);
				Assert.Null(foo.ComplexTypeWithConverter);
				Assert.Equal(TimeSpan.FromSeconds(5), foo.PingInterval);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenSavingSettings_ThenCanReadThem()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

				var foo = new Foo
				{
					StringProperty = "World",
					IntProperty = -1,
					DefaultValueIntProperty = -20,
					DefaultValueAsStringIntProperty = 25,
					EnumProperty = UriFormat.SafeUnescaped,
					DefaultValueEnumProperty = UriFormat.Unescaped,
					ComplexTypeWithConverter = new Bar("BarValue"),
					PingInterval = TimeSpan.FromMinutes(2),
				};

				manager.Save(foo);

				var saved = new Foo();
				manager.Read(saved);

				Assert.Equal("World", saved.StringProperty);
				Assert.Equal("Hello", saved.DefaultValueStringProperty);
				Assert.Equal(-1, saved.IntProperty);
				Assert.Equal(-20, saved.DefaultValueIntProperty);
				Assert.Equal(25, saved.DefaultValueAsStringIntProperty);
				Assert.Equal(UriFormat.SafeUnescaped, saved.EnumProperty);
				Assert.Equal(UriFormat.Unescaped, saved.DefaultValueEnumProperty);
				Assert.NotNull(saved.ComplexTypeWithConverter);
				Assert.Equal("BarValue", saved.ComplexTypeWithConverter.Value);
				Assert.Equal(TimeSpan.FromMinutes(2), saved.PingInterval);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenSavingInvalidSettings_ThenThrowsValidationException()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

				var foo = new Foo
				{
					StringProperty = "",
				};

				Assert.Throws<ValidationException>(() => manager.Save(foo));
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenSavingValueTypeSetting_ThenAlwaysSaveIfDifferentThanDefaultValue()
			{
                var manager = new SettingsManager(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

				var before = new Foo
				{
					IsEnabled = false,
					StringProperty = "Foo"
				};

				manager.Save(before);

				var after = new Foo();
				manager.Read(after);

				Assert.Equal(after.IsEnabled, false);
			}

			public class Foo
			{
				[Required(AllowEmptyStrings = false)]
				public string StringProperty { get; set; }
				[DefaultValue("Hello")]
				public string DefaultValueStringProperty { get; set; }
				public int IntProperty { get; set; }
				[DefaultValue(5)]
				public int DefaultValueIntProperty { get; set; }
				[DefaultValue("25")]
				public int DefaultValueAsStringIntProperty { get; set; }

				public UriFormat EnumProperty { get; set; }
				[DefaultValue("Unescaped")]
				public UriFormat DefaultValueEnumProperty { get; set; }

				[DefaultValue("00:00:05")]
				public TimeSpan PingInterval { get; set; }

				[DefaultValue(true)]
				public bool IsEnabled { get; set; }

				public Bar ComplexTypeWithConverter { get; set; }
			}

			[TypeConverter(typeof(BarConverter))]
			public class Bar
			{
				public Bar(string value)
				{
					this.Value = value;
				}

				public string Value { get; set; }

				private class BarConverter : TypeConverter
				{
					public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
					{
						return destinationType == typeof(string);
					}

					public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
					{
						return ((Bar)value).Value;
					}

					public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
					{
						return sourceType == typeof(string);
					}

					public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
					{
						return new Bar((string)value);
					}
				}
			}
		}
	}
}
