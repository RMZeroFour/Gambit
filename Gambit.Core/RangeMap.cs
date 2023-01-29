using System;
using System.Collections.Generic;
using System.Linq;

namespace Gambit.Core;

public class RangeMap<T>
{
	private readonly int[] keys;
	private readonly (int, T)[] values;

	public RangeMap (IDictionary<Range, T> source)
	{
		var sorted = source.Select(elt => (elt.Key.Start.Value, elt.Key.End.Value, elt.Value)).OrderBy(elt => elt.Item1);
		keys = sorted.Select(elt => elt.Item1).ToArray();
		values = sorted.Select(elt => (elt.Item2, elt.Item3)).ToArray();
	}

	public T GetValueOrDefault (int index, T def = default)
	{
		int valueIndex = Array.BinarySearch(keys, index);
		if (valueIndex >= 0)
		{
			return values[valueIndex].Item2;
		}
		else
		{
			int highestLowerIndex = ~valueIndex - 1;
			if (values[highestLowerIndex].Item1 > index)
				return values[highestLowerIndex].Item2;
		}
		return def;
	}
}