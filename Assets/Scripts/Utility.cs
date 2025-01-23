using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InTheDark.Prototypes
{
	public static class StringFormatUtility
	{
		public static string Color(this string message, string color)
		{
			var result = $"<color={color}>{message}</color>";

			return result;
		}

		public static string Color(this object target, string color)
		{
			var result = Color(target.ToString(), color);

			return result;
		}

		public static string Size(this string message, int size)
		{
			var result = $"<size={size}>{message}</size>";

			return result;
		}

		public static string Size(this object target, int size)
		{
			var result = Size(target.ToString(), size);

			return result;
		}
	}
}