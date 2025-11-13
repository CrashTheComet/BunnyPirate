using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
  public static bool InRange(object[] array, int i) => i >= 0 && i < array.Length;
  public static bool InRange(int length, int i) => i >= 0 && i < length;
}
