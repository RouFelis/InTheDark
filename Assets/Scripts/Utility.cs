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

		public static string Size(this string message, int size)
		{
			var result = $"<size={size}>{message}</size>";

			return result;
		}
	}
}