﻿using UnityEngine;
using System.Collections;

public class BitMaskAttribute : PropertyAttribute
{
	public System.Type propType;
	public BitMaskAttribute(System.Type aType)
	{
		propType = aType;
	}
}