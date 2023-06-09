﻿using System.IO;

namespace ThingsGateway.Foundation.Extension
{
    public static class StringExtension
    {
        /// <summary>
        /// 将字符串数组转换成字符串
        /// </summary>
        /// <param name="strArray">需要转换的字符串</param>
        /// <returns>合并完成的字符串</returns>
        public static string ArrayToString(this string[] strArray, string spitStr = "")
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i > 0)
                {
                    //分割符可根据需要自行修改
                    str.Append(spitStr);
                }
                str.Append(strArray[i]);
            }
            return str.ToString();
        }

        /// <summary>
        /// 从Base64转到数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ByBase64ToBytes(this string value)
        {
            return Convert.FromBase64String(value);
        }

        /// <summary>
        /// 将16进制的字符转换为数组。
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static byte[] ByHexStringToBytes(this string hexString, string splite = default)
        {
            if (!string.IsNullOrEmpty(splite))
            {
                hexString = hexString.Replace(splite, string.Empty);
            }

            if ((hexString.Length % 2) != 0)
            {
                hexString += " ";
            }
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        /// <summary>
        /// 将16进制的字符转换为int32。
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static int ByHexStringToInt32(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return default;
            }
            return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        public static string CombinePath(this string path, params string[] ps)
        {
            if (ps == null || ps.Length == 0)
            {
                return path;
            }

            if (path == null)
            {
                path = string.Empty;
            }

            foreach (string text in ps)
            {
                if (!text.IsNullOrEmpty())
                {
                    path = Path.Combine(path, text);
                }
            }

            return path;
        }

        /// <summary>
        /// 根据英文小数点进行切割字符串，去除空白的字符<br />
        /// </summary>
        /// <param name="str">字符串本身</param>
        public static string[] SplitDot(this string str)
        {
            return str.Split(new char[1]
{
      '.'
}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 只按第一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitFirst(this string str, char split)
        {
            List<string> s = new List<string>();
            int index = str.IndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }
    }
}