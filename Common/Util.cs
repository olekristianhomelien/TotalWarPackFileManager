using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace Common {
    /*
     * An Enumerator that relies on other enumerators to provide actual values.
     * This implementation will provide the functionality to enumerate the enumerators
     * to retrieve the values from them; subclasses will need to provide the contained
     * enumerators by implementing the NextEnumerator() method to provide actual
     * values (of type <T>).
     */
    public abstract class DelegatingEnumerator<T> : IEnumerator<T>, IDisposable {
        IEnumerator<T> currentEnumerator;
        public T Current {
            get {
                return currentEnumerator.Current;
            }
        }
        object System.Collections.IEnumerator.Current {
            get {
                return Current;
            }
        }
        public bool MoveNext() {
            bool result = true;
            if (currentEnumerator == null || !currentEnumerator.MoveNext()) {
                currentEnumerator = NextEnumerator();
                result = currentEnumerator != null && currentEnumerator.MoveNext();
            }
            return result;
        }
        public virtual void Reset() {
            if (currentEnumerator != null) {
                currentEnumerator.Dispose();
                currentEnumerator = null;
            }
        }
        protected abstract IEnumerator<T> NextEnumerator();

        public virtual void Dispose() {
            if (currentEnumerator != null) {
                currentEnumerator.Dispose();
            }
        }
    }

    // utilities for csv export
    public class CsvUtil
    {
        static string format = "\"{0}\"";

        public static string Format(string input)
        {
            string escaped = Regex.Escape(input.Trim());
            return string.Format(format, escaped)
                .Replace("\\ ", " ")
                .Replace("\\.", ".")
                .Replace("\\|", "|")
                .Replace("\\(", "(")
                .Replace("\\)", ")")
                .Replace("\\[", "[")
                ;
        }

        public static string Unformat(string formatted)
        {
            string result = Regex.Unescape(formatted);
            if (result.StartsWith("\""))
            {
                // remove one leading and trailing quote if present
                result = result.Substring(1, result.Length - 2);
            }
            return result.Trim();
        }
    }

    public class ObjectHelper
    {
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
