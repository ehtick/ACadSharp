﻿using System;
using System.ComponentModel;

namespace ACadSharp.Tests
{
	public static class TestVariables
	{
		public static bool LocalEnv { get { return EnvironmentVars.Get<bool>("LOCAL_ENV"); } }

		public static double Delta { get { return EnvironmentVars.Get<double>("DELTA"); } }

		public static int DecimalPrecision { get { return EnvironmentVars.Get<int>("DELTA"); } }

		public static bool AutocadConsoleCheck { get { return EnvironmentVars.Get<bool>("CONSOLE_CHECK"); } }

		static TestVariables()
		{
			EnvironmentVars.SetIfNull("LOCAL_ENV", "true");
			EnvironmentVars.SetIfNull("DELTA", "0.00001");
			EnvironmentVars.SetIfNull("DECIMAL_PRECISION", "5");
			EnvironmentVars.SetIfNull("CONSOLE_CHECK", "true");
		}
	}

	[Obsolete("Replace for the one in CSUtilities")]
	internal static class EnvironmentVars
	{
		public static void Set(string name, string value)
		{
			Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
		}

		public static void SetIfNull(string name, string value)
		{
			if (Get(name) == null)
			{
				Set(name, value);
			}
		}

		public static void SetIfNull(string name, string value, EnvironmentVariableTarget target)
		{
			if (Get(name) == null)
			{
				Set(name, value);
			}
		}

		public static string Get(string name)
		{
			return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
		}

		public static string Get(string name, EnvironmentVariableTarget target)
		{
			return Environment.GetEnvironmentVariable(name, target);
		}

		public static T Get<T>(string name)
		{
			string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
			return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value);
		}

		public static void Delete(string name)
		{
			Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.Process);
		}

		public static void Delete(string name, EnvironmentVariableTarget target)
		{
			Environment.SetEnvironmentVariable(name, null, target);
		}
	}
}
