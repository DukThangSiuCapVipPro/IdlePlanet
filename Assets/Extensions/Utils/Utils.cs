using UnityEngine;
using DG.Tweening;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections;

namespace ThangDD
{
    public static class Utils
    {
        /// <summary>
        /// Formats an integer number with two decimal places (N2).
        /// Example: 1234 → "1,234.00"
        /// </summary>
        public static string FormatToN2(this int value)
        {
            return value % 1 == 0 ? value.ToString("N0") : value.ToString("N2");
        }

        /// <summary>
        /// Formats a float number with two decimal places (N2).
        /// Example: 1234.567 → "1,234.57"
        /// </summary>
        public static string FormatToN2(this float value)
        {
            return value % 1 == 0 ? value.ToString("N0") : value.ToString("N2");
        }

        /// <summary>
        /// Formats a float number with two decimal places (N2).
        /// Example: 1234.567 → "1,234.57"
        /// </summary>
        public static string FormatToN2(this decimal value)
        {
            return value % 1 == 0 ? value.ToString("N0") : value.ToString("N2");
        }

        /// <summary>
        /// Formats large numbers with suffixes like K (thousand), M (million), B (billion).
        /// Example: 1500 → "1.50K", 2000000 → "2.00M"
        /// </summary>
        public static string FormatWithSuffix(this long number)
        {
            if (number < 1000) return number.ToString();

            string[] suffixes = { "", "K", "M", "B", "T", "Q" }; // Thousand, Million, Billion, Trillion, Quadrillion
            int suffixIndex = 0;
            double num = number;

            while (num >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                num /= 1000;
                suffixIndex++;
            }

            return $"{num:N2}{suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Formats numbers with letter suffixes: a, b, c, d... z
        /// 1000 = 1a, 1000a (1,000,000) = 1b, 1000b (1,000,000,000) = 1c, etc.
        /// Example: 1500 → "1.5a", 2500000 → "2.5b"
        /// </summary>
        public static string FormatWithLetters(this float number, int decimals = 1)
        {
            if (number < 1000) return number.ToString("F0");

            // Letter suffixes: a through z (26 letters)
            string[] suffixes = { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
                                  "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            int suffixIndex = 0;
            double num = number;

            while (num >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                num /= 1000;
                suffixIndex++;
            }

            // Format the number with specified decimal places
            string format = decimals > 0 ? $"F{decimals}" : "F0";
            return $"{num.ToString(format)}{suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Formats numbers with letter suffixes: a, b, c, d... z (double version)
        /// 1000 = 1a, 1000a (1,000,000) = 1b, 1000b (1,000,000,000) = 1c, etc.
        /// Example: 1500 → "1.5a", 2500000 → "2.5b"
        /// </summary>
        public static string FormatWithLetters(this double number, int decimals = 1)
        {
            return FormatWithLetters((float)number, decimals);
        }

        /// <summary>
        /// Formats numbers with letter suffixes: a, b, c, d... z (int version)
        /// 1000 = 1a, 1000a (1,000,000) = 1b, 1000b (1,000,000,000) = 1c, etc.
        /// Example: 1500 → "1.5a", 2500000 → "2.5b"
        /// </summary>
        public static string FormatWithLetters(this int number, int decimals = 1)
        {
            return FormatWithLetters((float)number, decimals);
        }

        /// <summary>
        /// Formats numbers with letter suffixes: a, b, c, d... z (long version)
        /// 1000 = 1a, 1000a (1,000,000) = 1b, 1000b (1,000,000,000) = 1c, etc.
        /// Example: 1500 → "1.5a", 2500000 → "2.5b"
        /// </summary>
        public static string FormatWithLetters(this long number, int decimals = 1)
        {
            return FormatWithLetters((float)number, decimals);
        }

        /// <summary>
        /// Smoothly animates a float value from current to target using DOTween.
        /// Calls onUpdate with the new value during the animation.
        /// Calls onComplete when the animation is finished.
        /// </summary>
        public static void AnimateNumber(this float currentValue, float targetValue, float duration,
                                         Action<float> onUpdate, Action onComplete = null)
        {
            DOTween.To(() => currentValue, x =>
            {
                onUpdate?.Invoke(x); // Updates value during animation
            }, targetValue, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke()); // Calls completion callback
        }

        /// <summary>
        /// Smoothly animates an integer value from current to target using DOTween.
        /// Calls onUpdate with the new integer value during the animation.
        /// Calls onComplete when the animation is finished.
        /// </summary>
        public static void AnimateNumber(this int currentValue, int targetValue, float duration,
                                         Action<int> onUpdate, Action onComplete = null)
        {
            DOTween.To(() => currentValue, x =>
            {
                onUpdate?.Invoke(Mathf.RoundToInt(x)); // Ensures integer rounding
            }, targetValue, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke()); // Calls completion callback
        }

        /// <summary>
        /// Get CSV data from public google sheet.
        /// Handle data, convert to List<string[]>.
        /// </summary>
        public static async Task<List<string[]>> GetDataFromGoogleSheet(string SHEET_ID, string SHEET_GID)
        {
            string url = $"https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=csv&gid={SHEET_GID}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string csvData = await client.GetStringAsync(url);
                    Debug.Log("Downloaded CSV Data:\n" + csvData);

                    List<string[]> rows = ParseCSV(csvData);
                    foreach (var row in rows)
                    {
                        Debug.Log(string.Join(" | ", row));
                    }
                    return rows;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to fetch sheet: " + ex.Message);
                return new List<string[]>();
            }
        }

        public static List<string[]> ParseCSV(string csv)
        {
            List<string[]> rows = new List<string[]>();

            using (StringReader reader = new StringReader(csv))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    rows.Add(SmartSplitCSVLine(line));
                }
            }

            return rows;
        }

        private static string[] SmartSplitCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            bool inBracket = false;
            string current = "";

            foreach (char c in line)
            {
                if (c == '\"') inQuotes = !inQuotes;
                if (c == '[') inBracket = true;
                if (c == ']') inBracket = false;

                if (c == ',' && !inQuotes && !inBracket)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
                result.Add(current.Trim());
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Trim('\"', ' ');
            }
            return result.ToArray();
        }

        public static List<T> Shuffle<T>(this List<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        /// <summary>
        /// Compares two lists for equality.
        /// Returns true if both lists contain the same instances in the same order.
        /// </summary>
        public static bool AreEqual<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Compares two lists for equality, ignoring order.
        /// Returns true if both lists contain the same instances, regardless of order.
        /// </summary>
        public static bool AreEquivalent<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            List<T> tempList = new(list2);

            foreach (var item in list1)
            {
                if (!tempList.Remove(item))
                    return false;
            }

            return tempList.Count == 0;
        }

        private class CoroutineRunner : MonoBehaviour { }
        private static CoroutineRunner _runner;
        private static void EnsureRunnerExists()
        {
            if (_runner == null)
            {
                GameObject runnerObj = new GameObject("DelayRunner");
                UnityEngine.Object.DontDestroyOnLoad(runnerObj);
                _runner = runnerObj.AddComponent<CoroutineRunner>();
            }
        }

        public static void Delay(float seconds, Action callback)
        {
            EnsureRunnerExists();
            _runner.StartCoroutine(DelayCoroutine(seconds, callback));
        }

        public static void DelayFrame(Action callback)
        {
            EnsureRunnerExists();
            _runner.StartCoroutine(DelayFrameCoroutine(callback));
        }

        private static IEnumerator DelayCoroutine(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        private static IEnumerator DelayFrameCoroutine(Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }
    }

    /// <summary>
    /// Represents very large numbers using coefficient + exponent system
    /// Perfect for idle games with exponential growth
    /// Stores numbers as: coefficient * (1000 ^ exponent)
    /// Example: 1.5a = coefficient: 1.5, exponent: 1
    /// </summary>
    [System.Serializable]
    public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>
    {
        [SerializeField] private double coefficient; // 1.0 to 999.999
        [SerializeField] private int exponent;       // 0 = no suffix, 1 = a, 2 = b, etc.

        private static readonly string[] suffixes = { "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
                                                      "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        public BigNumber(double value)
        {
            coefficient = value;
            exponent = 0;
            Normalize();
        }

        public BigNumber(double coef, int exp)
        {
            coefficient = coef;
            exponent = exp;
            Normalize();
        }

        /// <summary>
        /// Normalizes the number so coefficient is between 1 and 999.999
        /// </summary>
        private void Normalize()
        {
            if (coefficient == 0)
            {
                exponent = 0;
                return;
            }

            while (coefficient >= 1000 && exponent < suffixes.Length - 1)
            {
                coefficient /= 1000;
                exponent++;
            }

            while (coefficient < 1 && exponent > 0)
            {
                coefficient *= 1000;
                exponent--;
            }

            // Clamp to prevent overflow
            if (exponent >= suffixes.Length)
            {
                exponent = suffixes.Length - 1;
                coefficient = 999.999;
            }
        }

        #region Operators

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.exponent == b.exponent)
            {
                return new BigNumber(a.coefficient + b.coefficient, a.exponent);
            }

            // Convert to same exponent
            if (a.exponent > b.exponent)
            {
                double bConverted = b.coefficient * Math.Pow(1000, b.exponent - a.exponent);
                return new BigNumber(a.coefficient + bConverted, a.exponent);
            }
            else
            {
                double aConverted = a.coefficient * Math.Pow(1000, a.exponent - b.exponent);
                return new BigNumber(aConverted + b.coefficient, b.exponent);
            }
        }

        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            if (a.exponent == b.exponent)
            {
                return new BigNumber(a.coefficient - b.coefficient, a.exponent);
            }

            // Convert to same exponent
            if (a.exponent > b.exponent)
            {
                double bConverted = b.coefficient * Math.Pow(1000, b.exponent - a.exponent);
                return new BigNumber(a.coefficient - bConverted, a.exponent);
            }
            else
            {
                double aConverted = a.coefficient * Math.Pow(1000, a.exponent - b.exponent);
                return new BigNumber(aConverted - b.coefficient, b.exponent);
            }
        }

        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            return new BigNumber(a.coefficient * b.coefficient, a.exponent + b.exponent);
        }

        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.coefficient == 0)
                return new BigNumber(0);

            return new BigNumber(a.coefficient / b.coefficient, a.exponent - b.exponent);
        }

        public static BigNumber operator *(BigNumber a, double scalar)
        {
            return new BigNumber(a.coefficient * scalar, a.exponent);
        }

        public static BigNumber operator /(BigNumber a, double scalar)
        {
            if (scalar == 0)
                return new BigNumber(0);

            return new BigNumber(a.coefficient / scalar, a.exponent);
        }

        #endregion

        #region Comparison

        public static bool operator >(BigNumber a, BigNumber b)
        {
            if (a.exponent != b.exponent)
                return a.exponent > b.exponent;
            return a.coefficient > b.coefficient;
        }

        public static bool operator <(BigNumber a, BigNumber b)
        {
            if (a.exponent != b.exponent)
                return a.exponent < b.exponent;
            return a.coefficient < b.coefficient;
        }

        public static bool operator >=(BigNumber a, BigNumber b)
        {
            return a > b || a == b;
        }

        public static bool operator <=(BigNumber a, BigNumber b)
        {
            return a < b || a == b;
        }

        public static bool operator ==(BigNumber a, BigNumber b)
        {
            return a.exponent == b.exponent && Math.Abs(a.coefficient - b.coefficient) < 0.001;
        }

        public static bool operator !=(BigNumber a, BigNumber b)
        {
            return !(a == b);
        }

        #endregion

        #region Conversion

        public static implicit operator BigNumber(int value) => new BigNumber(value);
        public static implicit operator BigNumber(float value) => new BigNumber(value);
        public static implicit operator BigNumber(double value) => new BigNumber(value);

        public double ToDouble()
        {
            return coefficient * Math.Pow(1000, exponent);
        }

        public float ToFloat()
        {
            return (float)ToDouble();
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Formats the number with letter suffix (e.g., "1.5a", "2.3b")
        /// </summary>
        public string ToString(int decimals)
        {
            if (exponent == 0 && coefficient < 1000)
            {
                return coefficient.ToString("F0");
            }

            string format = decimals > 0 ? $"F{decimals}" : "F0";
            string suffix = exponent < suffixes.Length ? suffixes[exponent] : "z+";
            return $"{coefficient.ToString(format)}{suffix}";
        }

        public override string ToString()
        {
            return ToString(1);
        }

        #endregion

        #region Utility

        public bool IsZero()
        {
            return coefficient == 0 || (exponent == 0 && coefficient < 0.001);
        }

        public static BigNumber Max(BigNumber a, BigNumber b)
        {
            return a > b ? a : b;
        }

        public static BigNumber Min(BigNumber a, BigNumber b)
        {
            return a < b ? a : b;
        }

        public static BigNumber Zero => new BigNumber(0);
        public static BigNumber One => new BigNumber(1);

        #endregion

        #region Interface Implementations

        public int CompareTo(BigNumber other)
        {
            if (this > other) return 1;
            if (this < other) return -1;
            return 0;
        }

        public bool Equals(BigNumber other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is BigNumber)
                return Equals((BigNumber)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return coefficient.GetHashCode() ^ exponent.GetHashCode();
        }

        #endregion
    }
}
